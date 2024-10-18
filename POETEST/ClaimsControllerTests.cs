using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProgTest.Controllers;
using ProgTest.Data;
using ProgTest.Models;

namespace Prog_Part2_Test
{
    [TestFixture]
    public class ClaimsControllerTests
    {
        private ClaimsController _controller;
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {
            // Create a mock in-memory database with a unique name for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Ensures a unique database for each test
                .Options;

            // Create the context
            _context = new ApplicationDbContext(options);

            // Seed the database with test data
            _context.Claims.AddRange(new List<Claim>
            {
                new Claim { ClaimId = 1, LecturerName = "John Doe", HoursWorked = 10, HourlyRate = 20, ClaimDate = DateTime.Now, IsSubmitted = false },
                new Claim { ClaimId = 2, LecturerName = "Jane Smith", HoursWorked = 15, HourlyRate = 30, ClaimDate = DateTime.Now, IsSubmitted = true }
            });
            _context.SaveChanges();

            // Create the controller with the mocked context
            _controller = new ClaimsController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the context and controller
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithListOfClaims()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult; // Use as to cast
            Assert.That(viewResult, Is.Not.Null); // Ensure the result is not null

            var model = viewResult.Model as List<Claim>; // Cast the model
            Assert.That(model, Is.Not.Null); // Ensure the model is not null
            Assert.That(model.Count, Is.EqualTo(2)); // Verify that the number of claims is as expected
        }

        [Test]
        public async Task Create_ValidClaim_ReturnsRedirectToIndexAndAddsClaim()
        {
            // Arrange
            var newClaim = new Claim
            {
                ClaimId = 3,
                LecturerName = "Alice Johnson",
                HoursWorked = 12,
                HourlyRate = 25,
                ClaimDate = DateTime.Now,
                IsSubmitted = false
            };

            // Act
            var result = await _controller.Create(newClaim); // Assuming Create method accepts Claim as parameter

            // Assert
            var redirectResult = result as RedirectToActionResult; // Check for redirect result
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index")); // Ensure it redirects to Index

            // Verify the claim was added
            var claimsInDb = await _context.Claims.ToListAsync();
            Assert.That(claimsInDb.Count, Is.EqualTo(3)); // Ensure there are now 3 claims
            Assert.That(claimsInDb.Last().LecturerName, Is.EqualTo(newClaim.LecturerName)); // Check the last claim is the new one
        }

        [Test]
        public async Task Edit_ValidClaim_ReturnsRedirectToIndexAndUpdatesClaim()
        {
            // Arrange
            var existingClaim = await _context.Claims.FirstAsync(); // Get the first claim from the database
            existingClaim.LecturerName = "Updated Name"; // Update the claim details

            // Act
            var result = await _controller.Edit(existingClaim.ClaimId, existingClaim); // Assuming Edit method accepts ClaimId and Claim as parameters

            // Assert
            var redirectResult = result as RedirectToActionResult; // Check for redirect result
            Assert.That(redirectResult, Is.Not.Null); // Ensure it redirects
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index")); // Ensure it redirects to Index

            // Verify the claim was updated in the database
            var updatedClaim = await _context.Claims.FindAsync(existingClaim.ClaimId); // Retrieve the updated claim from the database
            Assert.That(updatedClaim, Is.Not.Null); // Ensure the updated claim exists
            Assert.That(updatedClaim.LecturerName, Is.EqualTo("Updated Name")); // Check that the lecturer name was updated
        }

        [Test]
        public async Task Create_InvalidClaim_ReturnsViewResult_WithModelError()
        {
            // Arrange
            var invalidClaim = new Claim
            {
                // Missing LecturerName, HoursWorked, and other required fields
                ClaimId = 4,
                HourlyRate = 0, // Invalid rate
                ClaimDate = DateTime.Now,
                IsSubmitted = false
            };

            // Simulate model validation error
            _controller.ModelState.AddModelError("LecturerName", "The LecturerName field is required.");

            // Act
            var result = await _controller.Create(invalidClaim);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null); // Ensure a ViewResult is returned
            Assert.That(viewResult.ViewName, Is.Null.Or.EqualTo("Create")); // Ensure it returns the Create view (default if not specified)

            // Check that the model state contains an error for LecturerName
            Assert.That(_controller.ModelState.IsValid, Is.False);
            Assert.That(_controller.ModelState["LecturerName"].Errors.Count, Is.EqualTo(1));

            // Verify that the invalid claim was not added to the database
            var claimsInDb = await _context.Claims.ToListAsync();
            Assert.That(claimsInDb.Count, Is.EqualTo(2)); // Database should still only have the original 2 claims
        }
    }
}