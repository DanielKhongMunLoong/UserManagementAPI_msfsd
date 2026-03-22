using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using System.Collections.Generic;
using System.Text.Json;
using System.Diagnostics;
using System.Data;
using System.Text;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    [ApiController]

    [Route("[controller]")]

    public class UserController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<UserEntry>> Get()
        {
            return StaffDatabase.StaffEntries;
        }

        [HttpGet("{id}")]
        public ActionResult<UserEntry> GetById(int id)
        {
            var user = StaffDatabase.StaffEntries.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return StatusCode(400, new { status = "Error", details = $"Failed to find current user with specified id {id}" });
            }
            return user;
        }
        // secure endpoint for user creation 
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<string>> Post()
        {
            // Manually read the raw request body stream
            using (var reader = new StreamReader(Request.Body))
            {
                string rawJson = await reader.ReadToEndAsync();

                try 
                {
                    var request = JsonSerializer.Deserialize<UserEntry>(rawJson, new JsonSerializerOptions
                    {   
                        PropertyNameCaseInsensitive = true 
                    });

                    if (request == null) 
                    {
                        return StatusCode(400, new { status = "Error", details = "Failed to read user information from JSON" });
                    } 
                    else
                    {
                        UserEntry newUser = new UserEntry
                        {
                            Id = StaffDatabase.GetNextId(),
                            Name = request.Name,
                            Role = request.Role,
                            Department = request.Department,
                            Age = request.Age
                        };
                        StaffDatabase.StaffEntries.Add(newUser);

                        String newUserDebug = $"Id: {newUser.Id}, Name: {newUser.Name}, Role: {newUser.Role}, Department: {newUser.Department}, Age: {newUser.Age}";
                        Console.WriteLine($"Post: Added: {newUserDebug}");
                        return StatusCode(200, new { status = "Success", details = "New user was added - " + newUserDebug });
                    }
                }
                catch (JsonException)
                {
                    return StatusCode(400, new { status = "Error", details = "Exception occurred, ensure JSON format is correct." });
                }
            }
        }
        // secure endpoint for user modification 
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<string>> Put(int id)
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string rawJson = await reader.ReadToEndAsync();

                try 
                {
                    var request = JsonSerializer.Deserialize<UserEntry>(rawJson, new JsonSerializerOptions
                    {   
                        PropertyNameCaseInsensitive = true 
                    });

                    if (request == null) 
                    {
                        return StatusCode(400, new { status = "Error", details = "Failed to read user information from JSON" });
                    } 
                    else
                    {
                        // Get the reference object from staff database
                        var currentUser = StaffDatabase.StaffEntries.FirstOrDefault(s => s.Id == id);
                        
                        if(currentUser != null)
                        {
                            if(!string.IsNullOrEmpty(request.Name))
                                currentUser.Name = request.Name;

                            if(!string.IsNullOrEmpty(request.Role))
                                currentUser.Role = request.Role;

                            if(!string.IsNullOrEmpty(request.Department))
                                currentUser.Department = request.Department;
                            
                            int parsedNumber;
                            if(request.Age != 0 && int.TryParse(request.Age.ToString(), out parsedNumber))
                                currentUser.Age = parsedNumber;
                            else
                                return StatusCode(400, new { status = "Error", details = $"Found current user id {id} but provided age in request is not an integer" });
                        }
                        else
                        {
                            return StatusCode(400, new { status = "Error", details = $"Failed to find current user with specified id {id}" });
                        }
                        
                        String currentUserDebug = $"Id: {currentUser.Id}, Name: {currentUser.Name}, Role: {currentUser.Role}, Department: {currentUser.Department}, Age: {currentUser.Age}";
                        Console.WriteLine($"Put: Updated: {currentUserDebug}");
                        return StatusCode(200, new { status = "Success", details = "Current user was modified - " + currentUserDebug });
                    }
                }
                catch (JsonException)
                {
                    return StatusCode(400, new { status = "Error", details = "Exception occurred, ensure JSON format is correct." });
                }
            }
        }
        // secure endpoint for user deletion
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<string>> Delete(int id)
        {
            //Get the reference object 
            var currentUser = StaffDatabase.StaffEntries.FirstOrDefault(s => s.Id == id);

            if(currentUser != null)
            {
                StaffDatabase.StaffEntries.Remove(currentUser);
            }	
            else
            {
                return StatusCode(400, new { status = "Error", details = $"Failed to find current user with specified id {id}" });
            }

            String currentUserDebug = $"Id: {currentUser.Id}, Name: {currentUser.Name}, Role: {currentUser.Role}, Department: {currentUser.Department}, Age: {currentUser.Age}";
            Console.WriteLine($"Delete: Removed: {currentUserDebug}");
            return StatusCode(200, new { status = "Success", details = "Current user was removed - " + currentUserDebug });
        }
        // endpoint for user login to issue token
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.UserName == Credentials.AdminUserName && request.Password == Credentials.AdminPassword)
            {
                // Setup Claims (User data inside the token)
                var claims = new[] {
                    new Claim(ClaimTypes.Name, Credentials.AdminUserName),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Credentials.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Create the Token
                var token = new JwtSecurityToken(
                    issuer: Credentials.Issuer,
                    audience: Credentials.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddHours(Credentials.ValidityHours),
                    signingCredentials: creds
                );

                return Ok(new { 
                    token = new JwtSecurityTokenHandler().WriteToken(token) 
                });
            }

            return Unauthorized("Invalid credentials");
        }
    }

}
