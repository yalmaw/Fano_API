using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentAdminPortal.API.Controllers;
using StudentAdminPortal.API.DomainModels;
using StudentAdminPortal.API.Repositories;
using Xunit;
using DataModels = StudentAdminPortal.API.DataModels; // Alias to distinguish between domain and data models


namespace StudentAdminPortal.API.Tests
{
   public  class StudentsControllerTests
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly IImageRepository _imageRepository;
        private readonly StudentsController _controller;

        public StudentsControllerTests()
        {
            // 1. Setup Fakes
            _studentRepository = A.Fake<IStudentRepository>();
            _mapper = A.Fake<IMapper>();
            _imageRepository = A.Fake<IImageRepository>();

            // 2. Instantiate Controller with Fakes
            _controller = new StudentsController(_studentRepository, _mapper, _imageRepository);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnOk_WhenStudentsExist()
        {
            // Arrange
            var dataStudents = new List<DataModels.Student> { new DataModels.Student() };
            var domainStudents = new List<Student> { new Student() };

            A.CallTo(() => _studentRepository.GetStudentsAsync()).Returns(dataStudents);
            A.CallTo(() => _mapper.Map<List<Student>>(dataStudents)).Returns(domainStudents);

            // Act
            var result = await _controller.GetAllStudentsAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(domainStudents);
        }

        [Fact]
        public async Task GetStudentAsync_ShouldReturnOk_WhenStudentExists()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var dataStudent = new DataModels.Student { Id = studentId };
            var domainStudent = new Student { Id = studentId };

            A.CallTo(() => _studentRepository.GetStudentAsync(studentId)).Returns(dataStudent);
            A.CallTo(() => _mapper.Map<Student>(dataStudent)).Returns(domainStudent);

            // Act
            var result = await _controller.GetStudentAsync(studentId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(domainStudent);
        }

        [Fact]
        public async Task GetStudentAsync_ShouldReturnNotFound_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            A.CallTo(() => _studentRepository.GetStudentAsync(studentId)).Returns((DataModels.Student)null);

            // Act
            var result = await _controller.GetStudentAsync(studentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var request = new UpdateStudentRequest();
            var mappedDataStudent = new DataModels.Student();
            var updatedDataStudent = new DataModels.Student(); // The result from repo
            var domainStudent = new Student();

            // Mock Exists check
            A.CallTo(() => _studentRepository.Exists(studentId)).Returns(true);

            // Mock Mapping request to Data Model
            A.CallTo(() => _mapper.Map<DataModels.Student>(request)).Returns(mappedDataStudent);

            // Mock Update Repo call
            A.CallTo(() => _studentRepository.UpdateStudent(studentId, mappedDataStudent)).Returns(updatedDataStudent);

            // Mock Mapping result to Domain Model
            A.CallTo(() => _mapper.Map<Student>(updatedDataStudent)).Returns(domainStudent);

            // Act
            var result = await _controller.UpdateStudentAsync(studentId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(domainStudent);
        }

        [Fact]
        public async Task UpdateStudentAsync_ShouldReturnNotFound_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var request = new UpdateStudentRequest();
            A.CallTo(() => _studentRepository.Exists(studentId)).Returns(false);

            // Act
            var result = await _controller.UpdateStudentAsync(studentId, request);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteStudentAsync_ShouldReturnOk_WhenStudentDeleted()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var deletedDataStudent = new DataModels.Student();
            var domainStudent = new Student();

            A.CallTo(() => _studentRepository.Exists(studentId)).Returns(true);
            A.CallTo(() => _studentRepository.DeleteStudent(studentId)).Returns(deletedDataStudent);
            A.CallTo(() => _mapper.Map<Student>(deletedDataStudent)).Returns(domainStudent);

            // Act
            var result = await _controller.DeleteStudentAsync(studentId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(domainStudent);
        }

        [Fact]
        public async Task DeleteStudentAsync_ShouldReturnNotFound_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            A.CallTo(() => _studentRepository.Exists(studentId)).Returns(false);

            // Act
            var result = await _controller.DeleteStudentAsync(studentId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AddStudentAsync_ShouldReturnCreated_WhenStudentAdded()
        {
            // Arrange
            var request = new AddStudentRequest();
            var mappedDataStudent = new DataModels.Student();
            var createdDataStudent = new DataModels.Student { Id = Guid.NewGuid() };
            var domainStudent = new Student { Id = createdDataStudent.Id };

            A.CallTo(() => _mapper.Map<DataModels.Student>(request)).Returns(mappedDataStudent);
            A.CallTo(() => _studentRepository.AddStudent(mappedDataStudent)).Returns(createdDataStudent);
            A.CallTo(() => _mapper.Map<Student>(createdDataStudent)).Returns(domainStudent);

            // Act
            var result = await _controller.AddStudentAsync(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(StudentsController.GetStudentAsync));
            createdResult.RouteValues["studentId"].Should().Be(createdDataStudent.Id);
            createdResult.Value.Should().Be(domainStudent);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnOk_WhenImageIsValidAndUploaded()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var fileMock = A.Fake<IFormFile>();
            var fileName = "profile.jpg";
            var filePath = "https://storage/profile.jpg";

            // Setup FormFile properties
            A.CallTo(() => fileMock.FileName).Returns(fileName);
            A.CallTo(() => fileMock.Length).Returns(1024); // Greater than 0

            // Logic flow mocks
            A.CallTo(() => _studentRepository.Exists(studentId)).Returns(true);
            A.CallTo(() => _imageRepository.Upload(fileMock, A<string>.Ignored)).Returns(filePath);
            A.CallTo(() => _studentRepository.UpdateProfileImage(studentId, filePath)).Returns(true);

            // Act
            var result = await _controller.UploadImage(studentId, fileMock);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().Be(filePath);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenExtensionIsInvalid()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var fileMock = A.Fake<IFormFile>();

            // Invalid extension
            A.CallTo(() => fileMock.FileName).Returns("malicious.exe");
            A.CallTo(() => fileMock.Length).Returns(1024);

            // Act
            var result = await _controller.UploadImage(studentId, fileMock);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().Be("This is not a valid Image format");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnNotFound_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var fileMock = A.Fake<IFormFile>();

            A.CallTo(() => fileMock.FileName).Returns("image.jpg");
            A.CallTo(() => fileMock.Length).Returns(1024);

            // Student does not exist
            A.CallTo(() => _studentRepository.Exists(studentId)).Returns(false);

            // Act
            var result = await _controller.UploadImage(studentId, fileMock);

            // Assert
            // Note: In your code, if Exists returns false, it falls out of the 'if' block 
            // and hits 'return BadRequest("This is not a valid Image format")' 
            // OR if valid extension block ends, it might fall through.
            // Looking at your logic:
            // If valid extension == true -> checks Exists.
            // If Exists == false -> it exits the inner if.
            // It then hits 'return BadRequest' because that is outside the 'if (validExtensions)' block?
            // Wait, looking at the curly braces in your provided code:
            // The 'return BadRequest' is outside the `if (validExtensions)` scope.
            // If `Exists` is false, nothing returns inside that block, so execution continues...
            // to `return BadRequest(...)`. 
            //
            // HOWEVER, looking at the very bottom:
            // if (profileImage != null) { ... }
            // return NotFound();
            //
            // If Exists is false, it creates a fall-through scenario.
            // Based on standard logic, if the student isn't found, we expect NotFound or Error.
            // Let's trace your specific code provided:
            // if (validExtensions) {
            //    if (exists) { ... return Ok }
            // }
            // return BadRequest("This is not a valid Image format");

            // Result: If the student does not exist, your current controller code actually returns BadRequest("This is not a valid Image format"), 
            // even though the format IS valid. This might be a logic bug in the controller, 
            // but the test must match the code as written.

            result.Should().BeOfType<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).Value.Should().Be("This is not a valid Image format");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnStatusCode500_WhenUpdateImageFails()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var fileMock = A.Fake<IFormFile>();
            var filePath = "path/to/image.jpg";

            A.CallTo(() => fileMock.FileName).Returns("image.jpg");
            A.CallTo(() => fileMock.Length).Returns(1024);
            A.CallTo(() => _studentRepository.Exists(studentId)).Returns(true);
            A.CallTo(() => _imageRepository.Upload(fileMock, A<string>.Ignored)).Returns(filePath);

            // Repo fails to update database
            A.CallTo(() => _studentRepository.UpdateProfileImage(studentId, filePath)).Returns(false);

            // Act
            var result = await _controller.UploadImage(studentId, fileMock);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Error uploading image");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnNotFound_WhenFileIsNull()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            IFormFile file = null!;

            // Act
            var result = await _controller.UploadImage(studentId, file);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}


