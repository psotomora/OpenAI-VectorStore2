using Microsoft.AspNetCore.Mvc;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.Models;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace OpenAIAssistantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly OpenAIService _openAiService;

        public DocumentController()
        {
            // Clave API de OpenAI
            var openAiApiKey = "tu_clave_api_de_openai";

            // Crear cliente OpenAI
            _openAiService = new OpenAIService(new OpenAiOptions { ApiKey = openAiApiKey });
        }

        // Subir documento y generar embeddings, almacenándolos en el Vector Store de OpenAI
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha proporcionado un archivo válido.");

            using var reader = new StreamReader(file.OpenReadStream());
            var fileContent = await reader.ReadToEndAsync();

            // Generar embedding con OpenAI
            var embeddingRequest = new EmbeddingCreateRequest
            {
                Model = Models.TextEmbeddingAda002,
                Input = fileContent
            };

            var embeddingResponse = await _openAiService.Embeddings.CreateEmbedding(embeddingRequest);
            var embeddingVector = embeddingResponse.Data.FirstOrDefault()?.Embedding;

            if (embeddingVector == null)
                return BadRequest("Error al generar el embedding.");

            // Insertar el vector en el almacenamiento de vectores de OpenAI
            var insertVectorResponse = await _openAiService.Vectors.UpsertAsync(new VectorsUpsertRequest
            {
                Vectors = new List<Vector>
                {
                    new Vector
                    {
                        Id = file.FileName, // ID único basado en el nombre del archivo
                        Values = embeddingVector // Embedding generado con OpenAI
                    }
                }
            });

            if (insertVectorResponse.Successful)
            {
                return Ok("Documento subido y vector almacenado correctamente.");
            }

            return BadRequest("Error al subir el vector al almacenamiento de OpenAI.");
        }

        // Consultar documentos en el almacenamiento vectorial de OpenAI
        [HttpPost("query")]
        public async Task<IActionResult> QueryDocuments([FromBody] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Consulta vacía.");

            // Generar embedding para la consulta
            var queryEmbeddingRequest = new EmbeddingCreateRequest
            {
                Model = Models.TextEmbeddingAda002,
                Input = query
            };

            var queryEmbeddingResponse = await _openAiService.Embeddings.CreateEmbedding(queryEmbeddingRequest);
            var queryEmbeddingVector = queryEmbeddingResponse.Data.FirstOrDefault()?.Embedding;

            if (queryEmbeddingVector == null)
                return BadRequest("Error al generar el embedding de la consulta.");

            // Realizar búsqueda en el almacenamiento vectorial de OpenAI
            var queryResponse = await _openAiService.Vectors.QueryAsync(new VectorsQueryRequest
            {
                TopK = 5,
                Vector = queryEmbeddingVector,
                IncludeMetadata = true
            });

            return Ok(queryResponse.Matches);
        }
    }
}