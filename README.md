# Overview

This repository contains an example REST style web service that hosts the functionality of the example licensing provider.

For more information on how the Carbon suite depends upon a licensing provider for authentication, see the [Carbon.Examples.Licensing.Provider][exlicprov] repository notes.

The web service can be customised by configuration variables at runtime (See below) or it can be cloned and used as the basis of a custom service to expose licensing functionality. The example licensing provider exposes an API defined by an interface named `ILicensingProvider`, and for a hosting web service to be useful it would be expected to publish most of the API. The example service does publish the complete API.

The service is intended to support other general purpose licence management applications written in whatever language or platform can consume a REST web service. Applications for the .NET platform can consume the example licensing provider directly without the need for a web service, but other application platforms running JavaScript, Python, etc, can benefit from using a platform neutral REST style web service as middleware.

----

## Response Conventions

Responses from the Licensing Web Service (REST style Web API) follow a simple convention where all responses are expected to produce only HTTP Status Code 200 (OK). This status indicates that the request was processed by the service application without any unexpected problems and a response was returned.

Any response code other than 200 indicates that there is some problem that cannot be handled by the service, such as a network failure, data corruption, or some other serious problem that should reported to the authors.

A 200 status only indicates that the service call was successful, it does not provide any information about the success of failure of the *business logic* of the call. To determine the status of the processing done by the call it is necessary to inspect some values that are present in every response. The body of every response when serialized as JSON has the following format.

```
{
  "hasError": bool,
  "errorCode": int?,
  "errorMessage": string,
  "errorDetail": string,
  "data": {
    :
    success response data
    :
  }
}
```

The `hasError` value indicates if the business logic of the call was processed successfully or not. If the value is True then `errorCode` with contain a numeric error code and `errorMessage` will contain an error summary message. The `errorDetail` value may optionally contain more detailed diagnostic information.

The `data` property contains a serialized value or object that is specific to each service call. It may contain a simple scalar value such as a number or string, or it might contain a serialized object with many properties. The value is expected to be null if there is a processing error.

### Example

A call is made to `POST /authentication/id` to request authentication of some credentials.

- **Success** &mdash; If the request was processed successfully, then the response status will be 200, the `errorCode` will be null and the `data` value will be a serialized `LicenceFull` object.

- **Failure** — If (for example) the password is incorrect, then the response status will 200, but the response `hasError` will be True, the `errorCode` will be 3 and the `errorMessage` will contain a short description of the error cause. The Swagger documentation for the web service describes the response codes that can possibly be returned by each endpoint.

----

## Authentication

The service defines two endpoints for clients to authenticate credentials against the licensing database.

`POST /authentication/id`  
`POST /authentication/name`

The POST body of the requests conatins a `AuthenticateNameId` and `AuthenticateNameRequest` serialized object respectively to provide the secure credentials to the provider.

A successful authentication response contains a `LicenceFull` object serialized into the body.

Note that a successful authentication does automatically grant any permissions to the licensing service itself. A success response is only meaningful to client applications who may use the user's licence information in whatever way is desirable in the client application.

----

## Authorisation

The web service uses a basic authorisation key present in request headers to allow access to the sensitive endpoints. Clients must provide a valid string in requests by adding a header with key `x-api-key`. The valid keys are defined in a private configuration value.

The web service was initially created for consumption by licensing management tools, so it does not implement any kind of roles based authorisation. A valid API key provides access to all service endpoints. In other words, it uses a *flat* authorisation model, also called "all or nothing" access.

> ⚠ To implement a role based model for authorisation it would be necessary for the service to return either a session Id or a [JWT][jwt]-like token, then manage the session internally and apply them via attributes to the endpoint methods. This has not been done for the example web service and is left as a possible exercise for any developers wishing to clone this repository.

> Technical Note : There is some experimental code in the AuthenticationController class to create a secure *signature* string which is returned in the authentication response. The signature behaves like a simple session token. The signature contains sufficient information for the licensing service to identify the client and it could inspect the user's roles to see if they are authorised for the endpoint. See the code comments for more information. As mentioned above, this experimental code could be replaced with a formal JWT or session processing library if desired.

----

# Configuration

Because the example web service is general purpose and not associated with any company, it provides a set of configuration values that can be used to customise its appearance and behaviour for different hosting environments. The following JSON sample shows the default values in the example project. The properties with a default of `null` MUST be set at runtime. The other default values are candidates for being overriden with custom values.

The follow JSON shows the default values in the `appsettings.json` file. These values are incomplete and mus be overriden with meaningful values at runtime.

```JSON
  "LicensingService": {
    "ProductKey": null,
    "ApiKeyList": null,
    "AdoConnect": null,
    "AzureConnect": null,
    "AzureContainer": null,
    "TwilioKey": null,
    "TwilioFromEmail": null,
    "TwilioFromName": null,
    "ChangePasswordEmailTemplateUri": null,
    "SignatureHours": 24,
    "SwaggerTitle": "Example Licensing Web Service",
    "SwaggerDescription": "An example REST-style Web API over the …truncated…",
    "SwaggerContactName": "Red Centre Software",
    "SwaggerContactUrl": "https://www.redcentresoftware.com/",
    "SwaggerContactEmail": "support@redcentresoftware.com"
  }
```

In a development environment like Visual Studio or Code (or a similar IDE) it's possible to locally override configuration values. For example, in Visual Studio this would be done using a project's *User Secrets* file.

In a production environment like hosting in Azure, configuration values would be set in the App Service's environment variables via the Portal or PowerShells scripts.

## Configuration Property Descriptions

| Property | Description |
| -- | -- |
| ProductKey | This value must be provided by Red Centre Software so that use of the example service is registered. |
| ApiKeyList | A comma-joined list of API keys that are valid in client request headers with name `x-api-key`. |
| AdoConnect | ADO connection string to the SQL Server database storing the licensing information. |
| AzureConnect | Azure storage account connection string where persistent data is stored. |
| AzureContainer | The Azure container name for storing persistent data. |
| TwilioKey | The API authorisation key for Twilio (SendGrid) to send emails. |
| TwilioFromEmail | The 'From' email address in emails. |
| TwilioFromName | The 'From' name in emails. |
| ChangePasswordEmailTemplateUri | The Uri of where the change password email body template can be found. |
| SignatureHours | How many hours an authentication response signature is valid. The signature can be used by client applications to make authenticated calls to the licensing service. |
| SwaggerTitle | Title to display at the top of the Swagger documentation page. |
| SwaggerDescription | Longer description to display under the Swagger page title. |
| SwaggerContactName | The contact or company name to display in the Swagger page Url and email links. |
| SwaggerContactUrl | The link address for the contact in the Swagger page. |
| SwaggerContactEmail | The email address for the contact in the Swagger page. |

Last updated: 28-May-2025

[exlicprov]: https://github.com/redcentre/Carbon.Examples.Licensing.Provider
[jwt]: https://en.wikipedia.org/wiki/JSON_Web_Token