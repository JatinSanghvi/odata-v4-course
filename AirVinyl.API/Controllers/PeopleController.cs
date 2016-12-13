using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class PeopleController : ODataController
    {
        private AirVinylDbContext dbContext = new AirVinylDbContext();

        [EnableQuery(MaxExpansionDepth = 3, MaxSkip = 10, MaxTop = 5, PageSize = 4)]
        public IHttpActionResult Get()
        {
            return Ok(dbContext.People);
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            IQueryable<Person> people = dbContext.People.Where(p => p.PersonId == key);

            if (!people.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(people));
        }

        [HttpGet]
        [ODataRoute("People({personId})/Email")]
        [ODataRoute("People({personId})/FirstName")]
        [ODataRoute("People({personId})/LastName")]
        [ODataRoute("People({personId})/DateOfBirth")]
        [ODataRoute("People({personId})/Gender")]
        public IHttpActionResult GetPersonProperty([FromODataUri] int personId)
        {
            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            string propertyName = Url.Request.RequestUri.Segments.Last();

            if (!person.HasProperty(propertyName))
            {
                return NotFound();
            }

            object propertyValue = person.GetValue(propertyName);

            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("People({personId})/VinylRecords")]
        public IHttpActionResult GetVinylRecords([FromODataUri] int personId)
        {
            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(dbContext.VinylRecords.Where(v => v.Person.PersonId == personId));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("People({personId})/Friends")]
        public IHttpActionResult GetFriends([FromODataUri] int personId)
        {
            Person person = dbContext.People.Include("Friends").FirstOrDefault(p => p.PersonId == personId);

            if (person == null || person.Friends == null)
            {
                return NotFound();
            }

            return Ok(person.Friends);
        }

        [HttpGet]
        [ODataRoute("People({personId})/Email/$value")]
        [ODataRoute("People({personId})/FirstName/$value")]
        [ODataRoute("People({personId})/LastName/$value")]
        [ODataRoute("People({personId})/DateOfBirth/$value")]
        [ODataRoute("People({personId})/Gender/$value")]
        public IHttpActionResult GetPersonRawProperty([FromODataUri] int personId)
        {
            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            string propertyName = Url.Request.RequestUri.Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');

            if (!person.HasProperty(propertyName))
            {
                return NotFound();
            }

            object propertyValue = person.GetValue(propertyName);

            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue.ToString());
        }

        public IHttpActionResult Post(Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dbContext.People.Add(person);
            dbContext.SaveChanges();

            return Created(person);
        }

        public IHttpActionResult Put([FromODataUri] int key, Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Person currentPerson = dbContext.People.FirstOrDefault(p => p.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();
            }

            person.PersonId = key;
            dbContext.Entry(currentPerson).CurrentValues.SetValues(person);
            dbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            patch.Patch(person);
            dbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Delete([FromODataUri] int key)
        {
            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            foreach (Person otherPerson in dbContext.People.Include("Friends"))
            {
                otherPerson.Friends.Remove(person);
            }

            dbContext.People.Remove(person);
            dbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("People({personId})/Friends/$ref")]
        public IHttpActionResult CreateLinkToFriend([FromODataUri] int personId, [FromBody] Uri friendUri)
        {
            Person person = dbContext.People.Include("Friends").FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            int friendId = Request.GetKeyValue<int>(friendUri);
            Person friend = dbContext.People.FirstOrDefault(p => p.PersonId == friendId);

            if (friend == null)
            {
                return NotFound();
            }

            if (person.Friends.Any(f => f.PersonId == friendId))
            {
                return BadRequest(string.Format($"Person with ID {friendId} is already friend of person with ID {personId}"));
            }

            person.Friends.Add(friend);
            dbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ODataRoute("People({personId})/Friends({currentFriendId})/$ref")]
        public IHttpActionResult UpdateLinkToFriend([FromODataUri] int personId, [FromODataUri] int currentFriendId, [FromBody] Uri newFriendUri)
        {
            Person person = dbContext.People.Include("Friends").FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            Person currentFriend = dbContext.People.FirstOrDefault(p => p.PersonId == currentFriendId);

            if (currentFriend == null)
            {
                return NotFound();
            }

            if (!person.Friends.Any(f => f.PersonId == currentFriendId))
            {
                return BadRequest(string.Format($"Person with ID {currentFriendId} is not friend of person with ID {personId}"));
            }

            int newFriendId = Request.GetKeyValue<int>(newFriendUri);
            Person newFriend = dbContext.People.FirstOrDefault(p => p.PersonId == newFriendId);

            if (newFriend == null)
            {
                return NotFound();
            }

            if (person.Friends.Any(f => f.PersonId == newFriendId))
            {
                return BadRequest(string.Format($"Person with ID {newFriendId} is already friend of person with ID {personId}"));
            }

            person.Friends.Remove(currentFriend);
            person.Friends.Add(newFriend);
            dbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE http://localhost:5810/odata/People({personId})/Friends/$ref?$id=http://localhost:5810/odata/People({friendId})
        [HttpDelete]
        [ODataRoute("People({personId})/Friends({friendId})/$ref")]
        public IHttpActionResult DeleteLinkToFriend([FromODataUri] int personId, [FromODataUri] int friendId)
        {
            Person person = dbContext.People.Include("Friends").FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            Person friend = dbContext.People.FirstOrDefault(p => p.PersonId == friendId);

            if (friend == null)
            {
                return NotFound();
            }

            if (!person.Friends.Any(f => f.PersonId == friendId))
            {
                return BadRequest(string.Format($"Person with ID {friendId} is not friend of person with ID {personId}"));
            }

            person.Friends.Remove(friend);
            dbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}