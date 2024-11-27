using Mango.Web.Models; // Importing the models from the Mango.Web.Models namespace.
using Mango.Web.Service.IService; // Importing the service interface from Mango.Web.Service.IService namespace.
using Newtonsoft.Json; // Importing Newtonsoft.Json to handle JSON serialization and deserialization.
using System.Net; // Importing the System.Net namespace for handling HTTP status codes.
using System.Text; // Importing System.Text for working with string encoding.
using static Mango.Web.Utility.SD; // Using a static import for accessing members of the SD class in Mango.Web.Utility.

namespace Mango.Web.Service // Defining the Mango.Web.Service namespace.
{
    public class BaseService : IBaseService // Defining the BaseService class that implements the IBaseService interface.
    {
        private readonly IHttpClientFactory _httpClientFactory; // Declaring a private read-only field for IHttpClientFactory to manage HTTP clients.
        private readonly ITokenProvider _tokenProvider; // Declaring a private read-only field for ITokenProvider to manage tokens.

        // Constructor to initialize the BaseService with IHttpClientFactory and ITokenProvider.
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory; // Assigning the injected IHttpClientFactory instance to the private field.
            _tokenProvider = tokenProvider; // Assigning the injected ITokenProvider instance to the private field.
        }

        // Async method to send an HTTP request and return a ResponseDto.
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {
                // Creating an HTTP client using IHttpClientFactory and naming it "MangoAPI".
                HttpClient client = _httpClientFactory.CreateClient("MangoAPI");
                // Creating a new HttpRequestMessage.
                HttpRequestMessage message = new();

                // Setting headers based on ContentType. If it's multipart form data, accept all content types.
                if (requestDto.ContentType == ContentType.MultipartFormData)
                {
                    message.Headers.Add("Accept", "*/*"); // Adding accept header to accept any type of content.
                }
                else
                {
                    message.Headers.Add("Accept", "application/json"); // Adding accept header for JSON content.
                }

                // If withBearer is true, add the Authorization header with the Bearer token.
                if (withBearer)
                {
                    var token = _tokenProvider.GetToken(); // Retrieving the token from ITokenProvider.
                    message.Headers.Add("Authorization", $"Bearer {token}"); // Adding Authorization header.
                }

                // Setting the request URI.
                message.RequestUri = new Uri(requestDto.Url); // Setting the request URL from the RequestDto.

                // Handling multipart form data content.
                if (requestDto.ContentType == ContentType.MultipartFormData)
                {
                    var content = new MultipartFormDataContent(); // Creating a new MultipartFormDataContent.

                    // Looping through each property of the Data object.
                    foreach (var prop in requestDto.Data.GetType().GetProperties())
                    {
                        var value = prop.GetValue(requestDto.Data); // Getting the value of the property.

                        // Checking if the value is a FormFile (e.g., an uploaded file).
                        if (value is FormFile)
                        {
                            var file = (FormFile)value; // Casting the value to FormFile.
                            if (file != null)
                            {
                                // Adding the file content to the request.
                                content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                            }
                        }
                        else
                        {
                            // Adding non-file data as string content to the request.
                            content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
                        }
                    }
                    // Setting the request content to the multipart form data.
                    message.Content = content;
                }
                else
                {
                    // For non-multipart content, serialize the Data object to JSON and add it to the request.
                    if (requestDto.Data != null)
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                    }
                }

                HttpResponseMessage? apiResponse = null; // Declaring a variable to hold the API response.

                // Switch statement to handle different API request types (POST, DELETE, PUT, GET).
                switch (requestDto.ApiType)
                {
                    case ApiType.POST:
                        message.Method = HttpMethod.Post; // Setting the method to POST.
                        break;
                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete; // Setting the method to DELETE.
                        break;
                    case ApiType.PUT:
                        message.Method = HttpMethod.Put; // Setting the method to PUT.
                        break;
                    default:
                        message.Method = HttpMethod.Get; // Default is GET.
                        break;
                }

                // Sending the HTTP request asynchronously.
                apiResponse = await client.SendAsync(message);

                // Handling the response based on status codes.
                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Not Found" }; // Return a response indicating "Not Found".
                    case HttpStatusCode.Forbidden:
                        return new() { IsSuccess = false, Message = "Access Denied" }; // Return a response indicating "Access Denied".
                    case HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized" }; // Return a response indicating "Unauthorized".
                    case HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Internal Server Error" }; // Return a response indicating "Internal Server Error".
                    default:
                        // For other responses, read the content and deserialize it into a ResponseDto.
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                        return apiResponseDto; // Return the deserialized ResponseDto.
                }
            }
            catch (Exception ex)
            {
                // In case of an exception, return a response with the error message.
                var dto = new ResponseDto
                {
                    Message = ex.Message.ToString(), // Set the exception message.
                    IsSuccess = false // Mark the request as unsuccessful.
                };
                return dto; // Return the response DTO with the error.
            }
        }
    }
}


/*
We use IHttpClientFactory to manage HttpClient instances efficiently. 
It prevents socket exhaustion, supports configuration, enables resilience 
with policies like retries, and makes the code cleaner by managing 
the lifecycle of HttpClient automatically.
*/