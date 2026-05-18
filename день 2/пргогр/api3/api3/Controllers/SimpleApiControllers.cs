using api3.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;

namespace PM20_2.Controllers
{
    // ---------------------------
    // Helpers / DTOs (без изменений)
    // ---------------------------
    public class LoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class RegisterRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string full_name { get; set; }
        public int role_id { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public int? department_id { get; set; }
    }

    public class ChangeStatusRequest
    {
        public string status { get; set; }
        public int? user_id { get; set; }
    }

    public class BatchStartRequest
    {
        public int user_id { get; set; }
        public decimal? actual_quantity_kg { get; set; }
    }

    public class BatchStepStartRequest
    {
        public int user_id { get; set; }
    }

    public class BatchStepFinishRequest
    {
        public int user_id { get; set; }
        public decimal? actual_temp_c { get; set; }
        public decimal? actual_pressure_bar { get; set; }
        public int? actual_duration_min { get; set; }
        public string operator_comment { get; set; }
    }

    public class QualityDecisionRequest
    {
        public int analyst_id { get; set; }
        public string decision { get; set; }
        public string analyst_comment { get; set; }
    }

    public class AuthResponse
    {
        public bool success { get; set; }
        public string token { get; set; }
        public object user { get; set; }
        public string message { get; set; }
    }

    public class DashboardDto
    {
        public int active_products { get; set; }
        public int active_recipes { get; set; }
        public int active_tech_maps { get; set; }
        public int orders_in_work { get; set; }
        public int batches_in_production { get; set; }
        public int batches_with_deviations { get; set; }
        public int batches_waiting_lab_decision { get; set; }
        public List<object> latest_events { get; set; }
    }

    internal static class SimpleHash
    {
        public static string Sha256(string value)
        {
            if (value == null) value = string.Empty;

            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    // ---------------------------
    // Улучшенный базовый CRUD-контроллер
    // ---------------------------
    public abstract class BaseCrudController<TEntity> : ApiController where TEntity : class
    {
        protected PM20_2Entities1 db = new PM20_2Entities1();

        protected abstract DbSet<TEntity> Set { get; }

        public BaseCrudController()
        {
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.ProxyCreationEnabled = false;
        }

        protected virtual int? GetId(TEntity entity)
        {
            var prop = typeof(TEntity).GetProperty("id");
            if (prop == null) return null;
            object value = prop.GetValue(entity, null);
            return value == null ? (int?)null : Convert.ToInt32(value);
        }

        // Универсальная обработка ошибок сохранения
        protected IHttpActionResult SaveWithErrorHandling(Action afterSave = null)
        {
            try
            {
                db.SaveChanges();
                afterSave?.Invoke();
                return null; // успех
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                if (msg.Contains("UNIQUE KEY") || msg.Contains("duplicate key") || msg.Contains("Ограничение уникальности"))
                    return Conflict();
                else if (msg.Contains("FOREIGN KEY") || msg.Contains("Внешний ключ"))
                    return BadRequest("Некорректная ссылка на связанный объект");
                else if (msg.Contains("CHECK") || msg.Contains("Ограничение проверки"))
                    return BadRequest("Неверное значение, нарушено ограничение CHECK");
                else if (msg.Contains("IDENTITY_INSERT"))
                    return BadRequest("Нельзя явно указывать значение автоинкрементного поля");
                else
                    return InternalServerError(ex);
            }
        }

        // GET api/controller
        public virtual IEnumerable<TEntity> Get()
        {
            return Set.ToList();
        }

        // GET api/controller/5
        public virtual IHttpActionResult Get(int id)
        {
            var entity = Set.Find(id);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        // PUT api/controller/5
        [ResponseType(typeof(void))]
        public virtual IHttpActionResult Put(int id, TEntity entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int? entityId = GetId(entity);
            if (entityId.HasValue && entityId.Value != id) return BadRequest("Идентификатор в теле не совпадает с URL");

            db.Entry(entity).State = EntityState.Modified;

            // Исключаем автозаполняемые поля, чтобы не затереть их
            var createdProp = typeof(TEntity).GetProperty("created_at");
            if (createdProp != null)
                db.Entry(entity).Property("created_at").IsModified = false;

            var passProp = typeof(TEntity).GetProperty("password_hash");
            if (passProp != null)
                db.Entry(entity).Property("password_hash").IsModified = false;

            var error = SaveWithErrorHandling();
            if (error != null) return error;

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/controller
        public virtual IHttpActionResult Post(TEntity entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Сбрасываем первичный ключ, чтобы избежать ошибок IDENTITY_INSERT
            var idProp = typeof(TEntity).GetProperty("id");
            if (idProp != null && idProp.PropertyType == typeof(int))
                idProp.SetValue(entity, 0);

            Set.Add(entity);

            var error = SaveWithErrorHandling();
            if (error != null) return error;

            int? id = GetId(entity);
            // Исправлено: использование Ok вместо CreatedAtRoute, чтобы избежать ошибки 500 при отсутствии маршрута
            return Ok(entity);
        }

        // DELETE api/controller/5
        public virtual IHttpActionResult Delete(int id)
        {
            var entity = Set.Find(id);
            if (entity == null) return NotFound();

            Set.Remove(entity);

            var error = SaveWithErrorHandling();
            if (error != null) return error;

            return Ok(entity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }

    // ---------------------------
    // Auth
    // ---------------------------
    [RoutePrefix("api/auth")]
    public class authController : ApiController
    {
        private PM20_2Entities1 db = new PM20_2Entities1();

        public authController()
        {
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.ProxyCreationEnabled = false;
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
                return BadRequest("username/password are required");

            string hash = SimpleHash.Sha256(request.password);

            var user = db.users.FirstOrDefault(u => u.username == request.username
                && u.password_hash == hash
                && u.is_active);

            if (user == null)
                return Content(HttpStatusCode.Unauthorized, new AuthResponse
                {
                    success = false,
                    message = "Неверный логин или пароль"
                });

            user.last_login = DateTime.Now;
            db.SaveChanges();

            return Ok(new AuthResponse
            {
                success = true,
                token = Guid.NewGuid().ToString("N"),
                user = new
                {
                    user.id,
                    user.username,
                    user.full_name,
                    user.role_id,
                    user.department_id,
                    user.email,
                    user.phone
                },
                message = "OK"
            });
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password) || string.IsNullOrWhiteSpace(request.full_name))
                return BadRequest("username/password/full_name are required");

            if (db.users.Any(u => u.username == request.username))
                return Content(HttpStatusCode.Conflict, "Пользователь с таким username уже существует");

            if (!db.roles.Any(r => r.id == request.role_id))
                return BadRequest("role_id не найден");

            if (request.department_id.HasValue && !db.departments.Any(d => d.id == request.department_id.Value))
                return BadRequest("department_id не найден");

            var user = new users
            {
                username = request.username,
                password_hash = SimpleHash.Sha256(request.password),
                full_name = request.full_name,
                role_id = request.role_id,
                email = request.email,
                phone = request.phone,
                department_id = request.department_id,
                is_active = true,
                created_at = DateTime.Now,
                last_login = null
            };

            db.users.Add(user);
            db.SaveChanges();

            return Ok(new
            {
                success = true,
                id = user.id,
                message = "Пользователь создан"
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }

    // ---------------------------
    // Простые справочники (без обязательных проверок)
    // ---------------------------
    public class rolesController : BaseCrudController<roles>
    {
        protected override DbSet<roles> Set { get { return db.roles; } }
    }

    public class departmentsController : BaseCrudController<departments>
    {
        protected override DbSet<departments> Set { get { return db.departments; } }
    }

    // ---------------------------
    // Продукты
    // ---------------------------
    public class productsController : BaseCrudController<products>
    {
        protected override DbSet<products> Set { get { return db.products; } }

        // POST с проверкой уникальности имени
        public override IHttpActionResult Post(products product)
        {
            if (string.IsNullOrWhiteSpace(product.name))
                return BadRequest("Название продукта обязательно");

            if (db.products.Any(p => p.name == product.name))
                return Conflict();

            // Устанавливаем статус по умолчанию, если не передан
            if (string.IsNullOrWhiteSpace(product.status))
                product.status = "draft";

            return base.Post(product);
        }

        [HttpGet]
        [Route("api/products/{id}/details")]
        public IHttpActionResult Details(int id)
        {
            var product = db.products.Find(id);
            if (product == null) return NotFound();

            var recipes = db.recipes.Where(r => r.product_id == id).ToList();
            var techMaps = db.tech_maps.Where(t => t.product_id == id).ToList();

            // Исправлено: используем ID рецептов, а не навигационное свойство
            var recipeIds = recipes.Select(r => r.id).ToList();
            var batches = db.batches.Where(b => recipeIds.Contains(b.recipe_id)).ToList();

            return Ok(new
            {
                product,
                recipes,
                techMaps,
                batches
            });
        }
    }

    public class raw_materialsController : BaseCrudController<raw_materials>
    {
        protected override DbSet<raw_materials> Set { get { return db.raw_materials; } }
    }

    // ---------------------------
    // Рецепты
    // ---------------------------
    public class recipesController : BaseCrudController<recipes>
    {
        protected override DbSet<recipes> Set { get { return db.recipes; } }

        private static readonly HashSet<string> ValidStatuses = new HashSet<string> { "draft", "active", "archived" };

        public override IHttpActionResult Post(recipes recipe)
        {
            if (recipe.product_id <= 0)
                return BadRequest("Не указан продукт (product_id)");
            if (!db.products.Any(p => p.id == recipe.product_id))
                return BadRequest("Продукт с таким ID не найден");
            if (recipe.version <= 0)
                return BadRequest("Версия рецепта должна быть положительным числом");

            if (!string.IsNullOrWhiteSpace(recipe.status) && !ValidStatuses.Contains(recipe.status))
                return BadRequest("Недопустимый статус рецепта");

            // Уникальность пары product_id + version
            if (db.recipes.Any(r => r.product_id == recipe.product_id && r.version == recipe.version))
                return Conflict();

            if (string.IsNullOrWhiteSpace(recipe.status))
                recipe.status = "draft";
            if (recipe.created_at == default(DateTime))
                recipe.created_at = DateTime.Now;

            return base.Post(recipe);
        }

        [HttpGet]
        [Route("api/recipes/{id}/full")]
        public IHttpActionResult Full(int id)
        {
            var recipe = db.recipes.Find(id);
            if (recipe == null) return NotFound();

            var components = db.recipe_components.Where(x => x.recipe_id == id).OrderBy(x => x.load_order).ToList();
            return Ok(new { recipe, components });
        }

        [HttpPost]
        [Route("api/recipes/{id}/activate")]
        public IHttpActionResult Activate(int id)
        {
            var recipe = db.recipes.Find(id);
            if (recipe == null) return NotFound();

            decimal sum = db.recipe_components
                .Where(x => x.recipe_id == id)
                .Select(x => (decimal?)x.percentage)
                .DefaultIfEmpty(0m)
                .Sum() ?? 0m;

            if (sum != 100m)
                return Content(HttpStatusCode.BadRequest, "Нельзя утвердить рецепт: сумма компонентов должна быть 100%");

            var otherActive = db.recipes.Any(r => r.product_id == recipe.product_id && r.status == "active" && r.id != id);
            if (otherActive)
                return Content(HttpStatusCode.BadRequest, "У продукта уже есть активная рецептура");

            recipe.status = "active";
            recipe.approved_at = DateTime.Now;

            var error = SaveWithErrorHandling();
            if (error != null) return error;

            return Ok(recipe);
        }

        [HttpPost]
        [Route("api/recipes/{id}/archive")]
        public IHttpActionResult Archive(int id)
        {
            var recipe = db.recipes.Find(id);
            if (recipe == null) return NotFound();

            recipe.status = "archived";
            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(recipe);
        }
    }

    public class recipe_componentsController : BaseCrudController<recipe_components>
    {
        protected override DbSet<recipe_components> Set { get { return db.recipe_components; } }

        public override IHttpActionResult Post(recipe_components component)
        {
            if (component.recipe_id <= 0)
                return BadRequest("Не указан рецепт (recipe_id)");
            if (!db.recipes.Any(r => r.id == component.recipe_id))
                return BadRequest("Рецепт с таким ID не найден");
            if (component.raw_material_id <= 0)
                return BadRequest("Не указано сырьё (raw_material_id)");
            if (!db.raw_materials.Any(rm => rm.id == component.raw_material_id))
                return BadRequest("Сырьё с таким ID не найдено");
            if (component.percentage <= 0 || component.percentage > 100)
                return BadRequest("Процент должен быть > 0 и <= 100");

            return base.Post(component);
        }

        [HttpGet]
        [Route("api/recipe-components/by-recipe/{recipeId}")]
        public IHttpActionResult ByRecipe(int recipeId)
        {
            return Ok(db.recipe_components.Where(x => x.recipe_id == recipeId).OrderBy(x => x.load_order).ToList());
        }
    }

    // ---------------------------
    // Технологические карты
    // ---------------------------
    public class tech_mapsController : BaseCrudController<tech_maps>
    {
        protected override DbSet<tech_maps> Set { get { return db.tech_maps; } }

        public override IHttpActionResult Post(tech_maps map)
        {
            if (map.product_id <= 0)
                return BadRequest("Не указан продукт (product_id)");
            if (!db.products.Any(p => p.id == map.product_id))
                return BadRequest("Продукт с таким ID не найден");
            if (map.version <= 0)
                return BadRequest("Версия должна быть положительным числом");

            if (!string.IsNullOrWhiteSpace(map.status) && !new[] { "draft", "active", "archived" }.Contains(map.status))
                return BadRequest("Недопустимый статус технологической карты");

            if (db.tech_maps.Any(t => t.product_id == map.product_id && t.version == map.version))
                return Conflict();

            if (string.IsNullOrWhiteSpace(map.status))
                map.status = "draft";
            if (map.created_at == default(DateTime))
                map.created_at = DateTime.Now;

            return base.Post(map);
        }

        [HttpGet]
        [Route("api/tech-maps/{id}/full")]
        public IHttpActionResult Full(int id)
        {
            var map = db.tech_maps.Find(id);
            if (map == null) return NotFound();

            var steps = db.tech_map_steps.Where(x => x.tech_map_id == id).OrderBy(x => x.step_order).ToList();
            return Ok(new { map, steps });
        }

        [HttpPost]
        [Route("api/tech-maps/{id}/activate")]
        public IHttpActionResult Activate(int id)
        {
            var map = db.tech_maps.Find(id);
            if (map == null) return NotFound();

            var otherActive = db.tech_maps.Any(t => t.product_id == map.product_id && t.status == "active" && t.id != id);
            if (otherActive)
                return Content(HttpStatusCode.BadRequest, "У продукта уже есть активная технологическая карта");

            map.status = "active";
            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(map);
        }

        [HttpPost]
        [Route("api/tech-maps/{id}/archive")]
        public IHttpActionResult Archive(int id)
        {
            var map = db.tech_maps.Find(id);
            if (map == null) return NotFound();

            map.status = "archived";
            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(map);
        }
    }

    public class tech_map_stepsController : BaseCrudController<tech_map_steps>
    {
        protected override DbSet<tech_map_steps> Set { get { return db.tech_map_steps; } }

        public override IHttpActionResult Post(tech_map_steps step)
        {
            if (step.tech_map_id <= 0)
                return BadRequest("Не указана технологическая карта (tech_map_id)");
            if (!db.tech_maps.Any(t => t.id == step.tech_map_id))
                return BadRequest("Технологическая карта с таким ID не найдена");
            if (string.IsNullOrWhiteSpace(step.step_name))
                return BadRequest("Название шага обязательно");
            if (string.IsNullOrWhiteSpace(step.step_type))
                return BadRequest("Тип шага обязателен");
            if (step.step_order <= 0)
                return BadRequest("Порядок шага должен быть положительным числом");

            return base.Post(step);
        }

        [HttpGet]
        [Route("api/tech-map-steps/by-map/{techMapId}")]
        public IHttpActionResult ByMap(int techMapId)
        {
            return Ok(db.tech_map_steps.Where(x => x.tech_map_id == techMapId).OrderBy(x => x.step_order).ToList());
        }
    }

    // ---------------------------
    // Производственные заказы
    // ---------------------------
    public class production_ordersController : BaseCrudController<production_orders>
    {
        protected override DbSet<production_orders> Set { get { return db.production_orders; } }

        private static readonly HashSet<string> ValidStatuses = new HashSet<string> { "draft", "planned", "in_progress", "completed", "archived" };

        public override IHttpActionResult Post(production_orders order)
        {
            if (string.IsNullOrWhiteSpace(order.order_number))
                return BadRequest("Номер заказа обязателен");
            if (db.production_orders.Any(o => o.order_number == order.order_number))
                return Conflict();
            if (order.recipe_id <= 0)
                return BadRequest("Не указан рецепт (recipe_id)");
            if (!db.recipes.Any(r => r.id == order.recipe_id))
                return BadRequest("Рецепт с указанным ID не найден");
            if (order.planned_quantity_kg <= 0)
                return BadRequest("Плановое количество должно быть больше нуля");
            if (!string.IsNullOrWhiteSpace(order.status) && !ValidStatuses.Contains(order.status))
                return BadRequest("Недопустимый статус заказа");

            if (string.IsNullOrWhiteSpace(order.status))
                order.status = "draft";

            return base.Post(order);
        }

        [HttpPost]
        [Route("api/production_orders/{id}/set-status")]   // исправлен дефис на подчёркивание
        public IHttpActionResult SetStatus(int id, ChangeStatusRequest request)
        {
            var order = db.production_orders.Find(id);
            if (order == null) return NotFound();
            if (request == null || string.IsNullOrWhiteSpace(request.status))
                return BadRequest("status is required");
            if (!ValidStatuses.Contains(request.status))
                return BadRequest("Недопустимый статус заказа");

            order.status = request.status;
            var error = SaveWithErrorHandling();
            if (error != null) return error;

            return Ok(order);
        }
    }

    // ---------------------------
    // Партии
    // ---------------------------
    public class batchesController : BaseCrudController<batches>
    {
        protected override DbSet<batches> Set { get { return db.batches; } }

        private static readonly HashSet<string> ValidStatuses = new HashSet<string> { "planned", "running", "completed", "aborted" };

        public override IHttpActionResult Post(batches batch)
        {
            if (string.IsNullOrWhiteSpace(batch.batch_number))
                return BadRequest("Номер партии обязателен");
            if (db.batches.Any(b => b.batch_number == batch.batch_number))
                return Conflict();
            if (batch.order_id <= 0)
                return BadRequest("Не указан заказ (order_id)");
            if (!db.production_orders.Any(o => o.id == batch.order_id))
                return BadRequest("Заказ с таким ID не найден");
            if (batch.recipe_id <= 0)
                return BadRequest("Не указан рецепт (recipe_id)");
            if (!db.recipes.Any(r => r.id == batch.recipe_id))
                return BadRequest("Рецепт с таким ID не найден");
            if (batch.tech_map_id <= 0)
                return BadRequest("Не указана технологическая карта (tech_map_id)");
            if (!db.tech_maps.Any(t => t.id == batch.tech_map_id))
                return BadRequest("Технологическая карта с таким ID не найдена");
            if (!string.IsNullOrWhiteSpace(batch.status) && !ValidStatuses.Contains(batch.status))
                return BadRequest("Недопустимый статус партии");
            if (batch.actual_quantity_kg.HasValue && batch.actual_quantity_kg.Value < 0)
                return BadRequest("Фактическое количество не может быть отрицательным");

            if (string.IsNullOrWhiteSpace(batch.status))
                batch.status = "planned";

            return base.Post(batch);
        }

        [HttpPost]
        [Route("api/batches/{id}/start")]
        public IHttpActionResult Start(int id, BatchStartRequest request)
        {
            var batch = db.batches.Find(id);
            if (batch == null) return NotFound();

            batch.status = "running";
            batch.start_time = DateTime.Now;
            if (request != null && request.actual_quantity_kg.HasValue)
                batch.actual_quantity_kg = request.actual_quantity_kg.Value;

            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(batch);
        }

        [HttpPost]
        [Route("api/batches/{id}/complete")]
        public IHttpActionResult Complete(int id, BatchStartRequest request)
        {
            var batch = db.batches.Find(id);
            if (batch == null) return NotFound();

            batch.status = "completed";
            batch.end_time = DateTime.Now;
            if (request != null && request.actual_quantity_kg.HasValue)
                batch.actual_quantity_kg = request.actual_quantity_kg.Value;

            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(batch);
        }

        [HttpGet]
        [Route("api/batches/{id}/details")]
        public IHttpActionResult Details(int id)
        {
            var batch = db.batches.Find(id);
            if (batch == null) return NotFound();

            var steps = db.batch_steps.Where(x => x.batch_id == id).OrderBy(x => x.step_order).ToList();
            var controls = db.quality_controls.Where(x => x.batch_id == id).OrderByDescending(x => x.analysis_date).ToList();
            return Ok(new { batch, steps, controls });
        }
    }

    // ---------------------------
    // Шаги партий
    // ---------------------------
    public class batch_stepsController : BaseCrudController<batch_steps>
    {
        protected override DbSet<batch_steps> Set { get { return db.batch_steps; } }

        public override IHttpActionResult Post(batch_steps step)
        {
            if (step.batch_id <= 0)
                return BadRequest("Не указана партия (batch_id)");
            if (!db.batches.Any(b => b.id == step.batch_id))
                return BadRequest("Партия с таким ID не найдена");
            if (string.IsNullOrWhiteSpace(step.step_name))
                return BadRequest("Название шага обязательно");
            if (step.step_order <= 0)
                return BadRequest("Порядок шага должен быть положительным числом");

            return base.Post(step);
        }

        [HttpPost]
        [Route("api/batch-steps/{id}/start")]
        public IHttpActionResult Start(int id, BatchStepStartRequest request)
        {
            var step = db.batch_steps.Find(id);
            if (step == null) return NotFound();

            step.started_at = DateTime.Now;
            if (request != null)
                step.started_by = request.user_id;

            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(step);
        }

        [HttpPost]
        [Route("api/batch-steps/{id}/finish")]
        public IHttpActionResult Finish(int id, BatchStepFinishRequest request)
        {
            var step = db.batch_steps.Find(id);
            if (step == null) return NotFound();

            step.completed_at = DateTime.Now;
            if (request != null)
            {
                step.completed_by = request.user_id;
                step.actual_temp_c = request.actual_temp_c;
                step.actual_pressure_bar = request.actual_pressure_bar;
                step.actual_duration_min = request.actual_duration_min;
                step.operator_comment = request.operator_comment;
            }

            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(step);
        }
    }

    // ---------------------------
    // Партии сырья
    // ---------------------------
    public class raw_material_batchesController : BaseCrudController<raw_material_batches>
    {
        protected override DbSet<raw_material_batches> Set { get { return db.raw_material_batches; } }

        public override IHttpActionResult Post(raw_material_batches batch)
        {
            if (string.IsNullOrWhiteSpace(batch.batch_number))
                return BadRequest("Номер партии сырья обязателен");
            if (db.raw_material_batches.Any(b => b.batch_number == batch.batch_number))
                return Conflict();
            if (batch.raw_material_id <= 0)
                return BadRequest("Не указано сырьё (raw_material_id)");
            if (!db.raw_materials.Any(rm => rm.id == batch.raw_material_id))
                return BadRequest("Сырьё с таким ID не найдено");
            if (batch.quantity <= 0)
                return BadRequest("Количество должно быть больше нуля");
            if (string.IsNullOrWhiteSpace(batch.unit))
                return BadRequest("Единица измерения обязательна");

            if (string.IsNullOrWhiteSpace(batch.status))
                batch.status = "pending";
            else if (!new[] { "pending", "in_analysis", "approved", "blocked" }.Contains(batch.status))
                return BadRequest("Недопустимый статус партии сырья");

            return base.Post(batch);
        }

        [HttpPost]
        [Route("api/raw-material-batches/{id}/approve")]
        public IHttpActionResult Approve(int id)
        {
            var item = db.raw_material_batches.Find(id);
            if (item == null) return NotFound();

            item.status = "approved";
            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(item);
        }

        [HttpPost]
        [Route("api/raw-material-batches/{id}/block")]
        public IHttpActionResult Block(int id)
        {
            var item = db.raw_material_batches.Find(id);
            if (item == null) return NotFound();

            item.status = "blocked";
            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(item);
        }
    }

    // ---------------------------
    // Контроль качества
    // ---------------------------
    public class quality_controlsController : BaseCrudController<quality_controls>
    {
        protected override DbSet<quality_controls> Set { get { return db.quality_controls; } }

        public override IHttpActionResult Post(quality_controls qc)
        {
            if (string.IsNullOrWhiteSpace(qc.sample_type))
                return BadRequest("Тип образца обязателен");
            if (string.IsNullOrWhiteSpace(qc.parameter_name))
                return BadRequest("Название параметра обязательно");

            // Хотя бы одна из ссылок должна быть заполнена
            if ((qc.batch_id ?? 0) <= 0 && (qc.raw_material_batch_id ?? 0) <= 0)
                return BadRequest("Не указана ни партия продукции, ни партия сырья");
            if (qc.batch_id.HasValue && !db.batches.Any(b => b.id == qc.batch_id.Value))
                return BadRequest("Партия продукции с таким ID не найдена");
            if (qc.raw_material_batch_id.HasValue && !db.raw_material_batches.Any(rmb => rmb.id == qc.raw_material_batch_id.Value))
                return BadRequest("Партия сырья с таким ID не найдена");

            if (!string.IsNullOrWhiteSpace(qc.result) && !new[] { "pass", "fail" }.Contains(qc.result))
                return BadRequest("Результат должен быть 'pass' или 'fail'");
            if (!string.IsNullOrWhiteSpace(qc.decision) && !new[] { "approved", "blocked" }.Contains(qc.decision))
                return BadRequest("Решение должно быть 'approved' или 'blocked'");

            return base.Post(qc);
        }

        [HttpPost]
        [Route("api/quality-controls/{id}/decision")]
        public IHttpActionResult Decision(int id, QualityDecisionRequest request)
        {
            var qc = db.quality_controls.Find(id);
            if (qc == null) return NotFound();
            if (request == null || string.IsNullOrWhiteSpace(request.decision))
                return BadRequest("decision is required");
            if (!new[] { "approved", "blocked" }.Contains(request.decision))
                return BadRequest("Решение должно быть 'approved' или 'blocked'");

            qc.decision = request.decision;
            qc.analyst_id = request.analyst_id;
            qc.analyst_comment = request.analyst_comment;

            if (request.decision == "approved")
                qc.result = "pass";
            else if (request.decision == "blocked")
                qc.result = "fail";

            var error = SaveWithErrorHandling();
            if (error != null) return error;
            return Ok(qc);
        }
    }

    public class audit_logController : BaseCrudController<audit_log>
    {
        protected override DbSet<audit_log> Set { get { return db.audit_log; } }
    }

    // ---------------------------
    // Dashboard / сводка
    // ---------------------------
    [RoutePrefix("api/dashboard")]
    public class dashboardController : ApiController
    {
        private PM20_2Entities1 db = new PM20_2Entities1();

        public dashboardController()
        {
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.ProxyCreationEnabled = false;
        }

        [HttpGet]
        [Route("summary")]
        public IHttpActionResult Summary()
        {
            var dto = new DashboardDto
            {
                active_products = db.products.Count(x => x.status == "active"),
                active_recipes = db.recipes.Count(x => x.status == "active"),
                active_tech_maps = db.tech_maps.Count(x => x.status == "active"),
                orders_in_work = db.production_orders.Count(x => x.status == "planned" || x.status == "in_progress"),
                batches_in_production = db.batches.Count(x => x.status == "planned" || x.status == "running"),
                batches_with_deviations = db.batch_steps
                    .Where(x => x.deviation_flag)
                    .Select(x => x.batch_id)
                    .Distinct()
                    .Count(),
                batches_waiting_lab_decision = db.quality_controls.Count(x => x.decision == null)
            };

            dto.latest_events = db.audit_log
                .OrderByDescending(x => x.changed_at)
                .Take(10)
                .Select(x => new
                {
                    x.id,
                    x.table_name,
                    x.record_id,
                    x.action,
                    x.changed_at,
                    x.changed_by
                })
                .ToList<object>();

            return Ok(dto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}