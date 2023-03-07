using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Globalization;

namespace ContactManager.API.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ContactsController(IConfiguration config)
        {
            _config = config;
        }


        [HttpGet]
        [Route("contacts")]
        public async Task<ActionResult<List<Contacts>>> GetAllContacts() 
        {
            try 
            {
                using var dapperConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                var contacts = await dapperConnection.QueryAsync<Contacts>("select * from contact_details");
                return Ok(contacts);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpGet]
        [Route("contacts/{cid}")]
        public async Task<ActionResult<Contacts>> GetContact(string cid)
        {
            try 
            {
                //implement notfound logic here 
                using var dapperConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                var contact = await dapperConnection.QueryFirstAsync<Contacts>("select * from contact_details where id =@paramId", new { paramId = cid });
                return Ok(contact);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost]
        [Route("contacts")]
        public async Task<ActionResult<List<Contacts>>> CreateHero(Contacts oCont) 
        {
            try 
            {
                using var dapperConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                int count = Convert.ToInt32(await dapperConnection.ExecuteScalarAsync("select count(*) from contact_details where email = @paramEmail", new { paramEmail = oCont.Email }));
                if (count >= 1)
                {
                    return BadRequest($"Email id already exists: {oCont.Email}");
                }
                else
                {
                    //SET: createddate(done), lastchangedate(done), hasbirthdaysoon(tbd)
                    DateTime createDate = DateTime.Now;
                    DateTime lastChangeDate = DateTime.Now;
                    string hasBirthdaySoon = "";
                    int remainingDaysForBday = GetDaysUntilBirthday(oCont.BirthDate);
                    if (remainingDaysForBday <= 14) { hasBirthdaySoon = "Yes"; }

                    if (string.IsNullOrEmpty(oCont.DisplayName))
                    {
                        oCont.DisplayName = $"{oCont.Salutation}. {oCont.FirstName} {oCont.LastName}";
                    }
                    DomainContactModel oCDom = new DomainContactModel();
                    oCDom.Salutation = oCont.Salutation;
                    oCDom.FirstName = oCont.FirstName;
                    oCDom.LastName = oCont.LastName;
                    oCDom.DisplayName = oCont.DisplayName;
                    oCDom.BirthDate = oCont.BirthDate;
                    oCDom.Email = oCont.Email;
                    oCDom.PhoneNumber = oCont.PhoneNumber;

                    oCDom.CreatedDate = createDate;
                    oCDom.LastChangeDate = lastChangeDate;
                    oCDom.HasBirthdaySoon = hasBirthdaySoon;
                    

                    await dapperConnection.ExecuteAsync("insert into contact_details" +
                        "(salutation, firstname, lastname, displayname, birthdate, createddate, lastchangedate, hasbirthdaysoon, email, phonenumber) values " +
                        "(@Salutation, @FirstName, @LastName, @DisplayName, @BirthDate, @CreatedDate, @LastChangeDate, @HasBirthDaySoon, @Email, @PhoneNumber)",
                        new { oCDom.Salutation, oCDom.FirstName, oCDom.LastName, oCDom.DisplayName, oCDom.BirthDate, oCDom.CreatedDate, oCDom.LastChangeDate, oCDom.HasBirthdaySoon, oCDom.Email, oCDom.PhoneNumber });
                }
                return Ok(await dapperConnection.QueryAsync<Contacts>("select * from contact_details"));
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("contacts")]
        public async Task<ActionResult<List<Contacts>>> UpdateContact(Contacts oCont) 
        {
            try 
            {
                using var dapperConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                int count = Convert.ToInt32(await dapperConnection.ExecuteScalarAsync("select count(*) from contact_details where id = @paramId", new { paramId = oCont.Id }));

                if (count == 1)
                {
                    string hasBirthdaySoon = "";
                    int remainingDaysForBday = GetDaysUntilBirthday(oCont.BirthDate);
                    if (remainingDaysForBday <= 14) { hasBirthdaySoon = "Yes"; }

                    DomainContactModel oCDom = new DomainContactModel();
                    oCDom.Salutation = oCont.Salutation;
                    oCDom.FirstName = oCont.FirstName;
                    oCDom.LastName = oCont.LastName;

                    if (string.IsNullOrEmpty(oCont.DisplayName))
                    {
                        oCont.DisplayName = $"{oCont.Salutation}. {oCont.FirstName} {oCont.LastName}";
                    }
                    oCDom.DisplayName = oCont.DisplayName;
                    oCDom.BirthDate = oCont.BirthDate;
                    oCDom.LastChangeDate = DateTime.Now;
                    oCDom.HasBirthdaySoon = hasBirthdaySoon;
                    oCDom.Email = oCont.Email;
                    oCDom.PhoneNumber = oCont.PhoneNumber;


                    await dapperConnection.ExecuteAsync("update contact_details set " +
                        "salutation=@Salutation," +
                        "firstname=@FirstName," +
                        "lastname=@LastName," +
                        "displayname=@DisplayName," +
                        "birthdate=@BirthDate," +
                        "lastchangedate=@LastChangeDate," +
                        "hasbirthdaysoon=@HasBirthDaySoon," +
                        "email=@Email," +
                        "phonenumber=@PhoneNumber " +
                        "where id = @Id",
                        new { oCont.Id, oCDom.Salutation, oCDom.FirstName, oCDom.LastName, oCDom.DisplayName, oCDom.BirthDate, oCDom.LastChangeDate, oCDom.HasBirthdaySoon, oCDom.Email, oCDom.PhoneNumber });

                    return Ok(await dapperConnection.QueryAsync<Contacts>("select * from contact_details where email =@paramEmail", new { paramEmail = oCont.Email }));
                }
                else 
                {
                    return NotFound($"Requested contact not found with email: {oCont.Email}");
                }
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message); 
            }
        }

        [HttpDelete]
        [Route("contacts/{cid}")]
        public async Task<ActionResult<List<Contacts>>> DeleteContact(int cid) 
        {
            try
            {
                using var dapperConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                int count = Convert.ToInt32(await dapperConnection.ExecuteScalarAsync("select count(*) from contact_details where id = @paramId", new { paramId = cid }));
                if (count == 1)
                {
                    await dapperConnection.ExecuteAsync("delete from contact_details where id=@paramId", new { paramId = cid });
                    return Ok(await dapperConnection.QueryAsync<Contacts>("select * from contact_details"));
                }
                else {
                    return NotFound($"Requested contact not found with Id: {cid}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static int GetDaysUntilBirthday(DateTime birthday)
        {
            var nextBirthday = birthday.AddYears(DateTime.Today.Year - birthday.Year);
            if (nextBirthday < DateTime.Today)
            {
                nextBirthday = nextBirthday.AddYears(1);
            }
            return (nextBirthday - DateTime.Today).Days;
        }


    }
}
