using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Core.Domain.Interfaces;
using PdfManagement.Core.Application.Services;
using PdfManagement.Infrastructure.Data.Context;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace PdfManagement.Tests
{
    public class PdfDocumentServiceTests
    {
        private readonly Mock<IPdfDocumentRepository> _mockRepository;
        private readonly Mock<IFileStorageService> _mockFileStorage;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        public PdfDocumentServiceTests()
        {
            _mockRepository = new Mock<IPdfDocumentRepository>();
            _mockFileStorage = new Mock<IFileStorageService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
        }

        [Fact]
        public async Task UploadPdfAsync_ValidPdf_ReturnsDocument()
        {
            // Arrange
            var userId = "user123";
            var filePath = "user123/test.pdf";
            var fileName = "test.pdf";
            
            _mockFileStorage.Setup(fs => fs.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(filePath);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.Length).Returns(1024);

            var expectedDocument = new PdfDocument
            {
                Id = 1,
                FileName = fileName,
                FilePath = filePath,
                ContentType = "application/pdf",
                FileSize = 1024,
                UploaderId = userId
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<PdfDocument>()))
                .ReturnsAsync(expectedDocument);

            var service = new PdfDocumentService(_mockRepository.Object, _mockFileStorage.Object, _mockUnitOfWork.Object);

            // Act
            var result = await service.UploadPdfAsync(mockFile.Object, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fileName, result.FileName);
            Assert.Equal(filePath, result.FilePath);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Equal(1024, result.FileSize);
            Assert.Equal(userId, result.UploaderId);
        }

        [Fact]
        public async Task UploadPdfAsync_InvalidFileType_ThrowsArgumentException()
        {
            // Arrange
            var userId = "user123";
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

            var service = new PdfDocumentService(_mockRepository.Object, _mockFileStorage.Object, _mockUnitOfWork.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                service.UploadPdfAsync(mockFile.Object, userId));
        }

        [Fact]
        public async Task GetUserPdfsAsync_ReturnsUserDocuments()
        {
            // Arrange
            var userId = "user123";
            
            var documents = new List<PdfDocument>
            {
                new PdfDocument 
                { 
                    Id = 1,
                    FileName = "test1.pdf", 
                    UploaderId = userId,
                    FilePath = "path/to/test1.pdf",
                    ContentType = "application/pdf",
                    FileSize = 1024
                },
                new PdfDocument 
                { 
                    Id = 2,
                    FileName = "test2.pdf", 
                    UploaderId = userId,
                    FilePath = "path/to/test2.pdf",
                    ContentType = "application/pdf",
                    FileSize = 2048
                }
            };
            
            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(documents);
            
            var service = new PdfDocumentService(_mockRepository.Object, _mockFileStorage.Object, _mockUnitOfWork.Object);

            // Act
            var result = await service.GetUserPdfsAsync(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, doc => Assert.Equal(userId, doc.UploaderId));
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_ValidDocument_ReturnsToken()
        {
            // Arrange
            var userId = "user123";
            var documentId = 1;
            
            var document = new PdfDocument 
            { 
                Id = documentId,
                FileName = "test.pdf", 
                UploaderId = userId,
                FilePath = "path/to/test.pdf",
                ContentType = "application/pdf",
                FileSize = 1024
            };

            var expectedToken = new PdfAccessToken
            {
                Id = 1,
                DocumentId = documentId,
                Token = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            
            _mockRepository.Setup(r => r.GetByIdAsync(documentId))
                .ReturnsAsync(document);
                
            _mockRepository.Setup(r => r.CreateAccessTokenAsync(It.IsAny<PdfAccessToken>()))
                .ReturnsAsync(expectedToken);
            
            var service = new PdfDocumentService(_mockRepository.Object, _mockFileStorage.Object, _mockUnitOfWork.Object);

            // Act
            var result = await service.GenerateAccessTokenAsync(documentId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentId, result.DocumentId);
            Assert.False(result.IsRevoked);
            Assert.NotEqual(Guid.Empty, result.Token);
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_InvalidDocument_ThrowsArgumentException()
        {
            // Arrange
            var userId = "user123";
            var documentId = 999; // Non-existent document
            
            _mockRepository.Setup(r => r.GetByIdAsync(documentId))
                .ReturnsAsync((PdfDocument)null);
                
            var service = new PdfDocumentService(_mockRepository.Object, _mockFileStorage.Object, _mockUnitOfWork.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                service.GenerateAccessTokenAsync(documentId, userId));
        }
    }
}
