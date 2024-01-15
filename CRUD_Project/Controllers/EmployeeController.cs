using CRUD_Project.DataAccessLayer;
using CRUD_Project.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CRUD_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IDbHelper m_oDbHelper;
        public EmployeeController(IDbHelper dbHelper)
        {
            m_oDbHelper = dbHelper;
        }
        // GET: api/<EmployeeController>
        [HttpGet]
        public ActionResult<IEnumerable<Employee>> Get()
        {
            return m_oDbHelper.Query<Employee>().ToList();
        }

        // GET api/<EmployeeController>/5
        [HttpGet("{id}")]
        public ActionResult<Employee> Get(string id)
        {
            var result = m_oDbHelper.Find<Employee>(id);
            if (result is null)
            {
                return NotFound("找不到資源");
            }
            return result;
        }

        // POST api/<EmployeeController>
        [HttpPost]
        public ActionResult Post([FromBody] Employee value)
        {
            try
            {
                bool bResult = m_oDbHelper.Add<Employee>(value);
                if (!bResult)
                {
                    return Problem("新增失敗！", statusCode: 500);
                }
            }
            catch (Exception ex)
            {
                return Problem("新增失敗！", statusCode: 500);
            }

            return CreatedAtAction(nameof(Get), new
            {
                id = value.EmployeeId
            }, value);
        }

        // PUT api/<EmployeeController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Employee value)
        {
            if (id != value.EmployeeId)
            {
                return BadRequest();
            }
            try
            {
                 m_oDbHelper.Update<Employee>(value);
            }
            catch (Exception)
            {
                //更新失敗的原因可能是找不到ID
                if (m_oDbHelper.Find<Employee>(value.EmployeeId) == null)
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(500, "存取失敗！");
                }
            }
            return NoContent();
        }

        // DELETE api/<EmployeeController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var oDeleteEntity = m_oDbHelper.Find<Employee>(id);
            if(oDeleteEntity != null)
            {
                m_oDbHelper.Delete<Employee>(oDeleteEntity);
            }
            else
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
