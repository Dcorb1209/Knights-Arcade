﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnightsArcade.Infrastructure.Authentication;
using KnightsArcade.Infrastructure.Logic;
using KnightsArcade.Models;
using KnightsArcade.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace KnightsArcade.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RestrictedController : Controller
    {
        private readonly RDSLogic _rdsLogic;
        private readonly EC2Logic _ec2Logic;
        private readonly SMTPLogic _smtpLogic;
        private readonly ValidateJWT _validation;
        private readonly ILogger<RestrictedController> _logger;

        public RestrictedController(RDSLogic rdsLogic, ILogger<RestrictedController> logger, EC2Logic ec2Logic, ValidateJWT validation, SMTPLogic smtpLogic)
        {
            _rdsLogic = rdsLogic;
            _logger = logger;
            _ec2Logic = ec2Logic;
            _smtpLogic = smtpLogic;
            _validation = validation;
        }

        /// <summary>
        /// Get info of controller.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);

				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					return Ok("Knights Arcade Restricted");
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Start EC2 for automated testing.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success.</response> 
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error starting EC2.</response> 
        [HttpPut("aws/ec2/start")]
        public IActionResult PutStartAutomatedTestingEC2(bool start)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					_ec2Logic.StartAutomatedTestingEC2();
                    return Ok();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Stop EC2 for automated testing.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success.</response>  
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error stopping EC2.</response> 
        [HttpPut("aws/ec2/stop")]
        public IActionResult PutStopAutomatedTestingEC2(bool stop)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					_ec2Logic.StopAutomatedTestingEC2();
                    return Ok();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Creates the new entry in all 4 database tables: Games, Submissions, Tests, and TestsQueue.
        /// </summary>
        /// <remarks>
        /// The only way to post to a database table is through this method. You can only post to all tables.
        /// </remarks>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        /// <response code="201">Created.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="409">Duplicate gameName entry.</response>  
        /// <response code="500">Error.</response>  
        [HttpPost("rds/newentry")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult PostNewEntry([FromBody] GamesEntry newEntry)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (_validation.CheckValidation(accessToken.ToString()))
                {
                    Tuple<Games, int> tuple = _rdsLogic.PostNewEntry(newEntry);
                    if (tuple.Item2 == 1)
                    {
                        return StatusCode(409, "That game name already exists.");
                    }
                    _ec2Logic.StartAutomatedTestingEC2();
                    return StatusCode(201, tuple.Item1);
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Creates the new entry in all 4 database tables: Games, Submissions, Tests, and TestsQueue.
        /// </summary>
        /// <remarks>
        /// The only way to post to a database table is through this method. You can only post to all tables.
        /// </remarks>
        /// <param name="updateEntry"></param>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="409">Duplicate gameName entry.</response>  
        /// <response code="500">Error.</response>  
        [HttpPut("rds/resubmit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult PutNewEntry([FromBody] GamesEntry updateEntry)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
                if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (_validation.CheckValidation(accessToken.ToString()))
                {
                    _rdsLogic.PutNewEntry(updateEntry);
                    _ec2Logic.StartAutomatedTestingEC2();
                    return StatusCode(200);
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Updates the object in the Games database table.
        /// </summary>
        /// <remarks>
        /// REQUIRED: gameId.
        /// 
        /// Fields left in null are ignored when updating a database column for an entry. 
        /// Using this endpoint also updates its entry corresponding columns in the submissions table.
        /// You CANNOT update: gameId, gameName, gameCreatorId, gameCreatorName, gameSubmissionDateUtc fields.
        /// 
        /// Example
        /// PUT
        /// {
        ///     "gameId" : "5",
        ///     "gameName" : "Cool Guns 9001",
        ///     "gameCreatorId" : null,
        ///     "gameCreatorName" : "Zak",
        ///     "gameDescription" : "This game is the dopest.",
        ///     "gameControls" : "LOL figure it out.",
        ///     "gameVideoLink" : null,
        ///     "gameGenres" : null,
        ///     "gameStatus" : null,
        ///     "gameOnArcade" : null,
        ///     "gamePath" : null,
        ///     "gameImage0" : null,
        ///     "gameImage1" : null,
        ///     "gameImage2" : "somefakeurl.com/cool_math.jpeg",
        ///     "gameImage3" : null,
        ///     "gameImage4" : null,
        ///     "gameSubmissionsDateUtc" : null,
        ///     "gameReviewDateUtc" : null
        /// }
        /// -- In this example only the gameDescription, gameControls and gameImage2 columns will be updated for the entry in the database.
        /// </remarks>
        /// <param name="game"></param>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPut("rds/games/game")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PutGames(GamesEntry game)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					_rdsLogic.PutGamesEntry(game);
                    return Ok();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Deletes the object in the Games database table.
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        /// <response code="204">No Content.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpDelete("rds/games/game")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult DeleteGames(int gameId)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
                if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (_validation.CheckValidation(accessToken.ToString()))
                {
                    _rdsLogic.DeleteGamesEntry(gameId);
                    return NoContent();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Updates the object in the Tests database table.
        /// </summary>
        /// <remarks>
        /// REQUIRED: gameId.
        /// 
        /// Fields left in null are ignored when updating a database column for an entry. 
        /// You CANNOT update: gameId. 
        /// </remarks>
        /// <param name="test"></param>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPut("rds/tests/test")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PutTests([FromBody] Tests test)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
                if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

               if (_validation.CheckValidation(accessToken.ToString()))
               {
                    _rdsLogic.PutTests(test);
                    return Ok();
               }

               return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Updates the object retryCount column by one in the TestsQueue database table.
        /// </summary>
        /// <remarks>
        /// Only updates the retryCount column by one.
        /// </remarks>
        /// <param name="testsQueue"></param>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPut("rds/testsqueue/testqueue")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PutTestsQueue([FromBody] TestsQueue testsQueue)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
                
				if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (_validation.CheckValidation(accessToken.ToString()))
                {
                    _rdsLogic.PutTestsQueue(testsQueue);
                    return Ok();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Deletes the object in the TestsQueue database table.
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        /// <response code="204">No Content.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpDelete("rds/testsqueue/testqueue")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult DeleteTestsQueue(int gameId)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
                if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (_validation.CheckValidation(accessToken.ToString()))
                {
                    _rdsLogic.DeleteTestsQueue(gameId);
                    return NoContent();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Adds the user to the users table.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <response code="201">Created.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPost("rds/users/user")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PostUser([FromBody] Users user)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					_rdsLogic.PostUser(user);
                    return StatusCode(201);
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Updates the user to the users table.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPut("rds/users/user")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PutUser([FromBody] Users user)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					_rdsLogic.PutUser(user);
                    return Ok();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Adds the testing log to the testing log table.
        /// </summary>
        /// <param name="testingLog"></param>
        /// <returns></returns>
        /// <response code="201">Created.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPost("rds/testinglog/testinglog")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PostTestingLog(TestingLog testingLog)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
                if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (_validation.CheckValidation(accessToken.ToString()))
                {
                    _rdsLogic.PostTestingLog(testingLog);
                    return StatusCode(201);
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Adds the arcade machine to the arcademachines table.
        /// </summary>
        /// <param name="arcadeMachine"></param>
        /// <returns></returns>
        /// <response code="201">Created.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPost("rds/arcademachines/arcademachine")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PostArcadeMachine(ArcadeMachine arcadeMachine)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					ArcadeMachine arcade = _rdsLogic.PostArcadeMachine(arcadeMachine);
                    return StatusCode(201, arcade);
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Updates the arcade machine in the arcademachines table.
        /// </summary>
        /// <param name="arcadeMachine"></param>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPut("rds/arcademachines/arcademachine")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult PutArcadeMachine(ArcadeMachine arcadeMachine)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					_rdsLogic.PutArcadeMachine(arcadeMachine);
                    return StatusCode(200);
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Deletes arcade machine from the arcademachines table.
        /// </summary>
        /// <param name="arcadeMachineId"></param>
        /// <returns></returns>
        /// <response code="204">No content.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpDelete("rds/arcademachines/arcademachine")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult DeleteArcadeMachine(int arcadeMachineId)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
				if (accessToken.Count() == 0)
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
				{
					return StatusCode(401, "Empty or no authorization header.");
				}

				if (_validation.CheckValidation(accessToken.ToString()))
				{
					_rdsLogic.DeleteArcadeMachine(arcadeMachineId);
                    return StatusCode(204);
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <response code="200">Success.</response>
        /// <response code="401">Empty or no authorization header.</response>
        /// <response code="403">Invalid access token given.</response>
        /// <response code="500">Error.</response>  
        [HttpPost("smtp/gmail/sendemail")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult SendEmail([FromBody] Email email)
        {
            try
            {
                StringValues accessToken = new StringValues();
                Request.Headers.TryGetValue("Authorization", out accessToken);
                if (accessToken.Count() == 0)
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }

                if (accessToken.FirstOrDefault().ToString() == null || accessToken.FirstOrDefault().ToString() == "")
                {
                    return StatusCode(401, "Empty or no authorization header.");
                }
                
                if (_validation.CheckValidation(accessToken.ToString()))
                {
                    _smtpLogic.SendEmail(email);
                    return Ok();
                }

                return StatusCode(403, "This is an invalid access token.");
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode(500, e.Message);
            }
        }
    }
}