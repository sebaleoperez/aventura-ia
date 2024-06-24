# aventura-ia / adventure-ai
(English below)

Este repositorio es un "expermiento" con el que busco:
- Aprender como funciona el servicio de Azure Open AI, como gestionarlo y como integrarlo en un codigo .NET
- Disponer de un "sandbox" donde probar las nuevas versiones y funcionalidades de Azure Open AI y de .NET
- Crear un proyecto de codigo abierto que pueda ser util para otros desarrolladores que quieran integrar Azure Open AI en sus aplicaciones .NET
- Que sea algo fuera de lo tradicional y que sea divertido, por eso elegi un juego :)

# Por qué una aplicación de consola?
Principalmente porque es un proyecto de prueba y porque es más sencillo de implementar y de probar. Ademas, tiene ese aire "vintage" que me hace recordar a los antiguos juegos de aventuras basados en texto.

Dicho esto, mi objetivo es expandir el multiverso de la "Aventura IA" a otros tipos de aplicaciones, como aplicaciones web, aplicaciones móviles, etc. Todo por supuesto desarrollado en .NET.

# Requerimientos
- .NET 8.0 instalado
- Una suscripción a Azure con acceso a Azure Open AI (ver debajo)
- Deployments en esta suscripcion de un modelo generativo de texto como GPT y un modelo generativo de imagen como DALL-E

# Cómo obtener una suscripción a Azure con acceso a Azure Open AI
- Si no tienes una suscripción a Azure, puedes obtener una gratis en https://azure.microsoft.com/es-es/free/
- Una vez tengas tu suscripción, puedes activar el servicio de Azure Open AI siguiendo las instrucciones en https://learn.microsoft.com/en-us/legal/cognitive-services/openai/limited-access

# Configuración de los recursos
Se deben configurar las siguientes variables de entorno:
"AZURE_OPENAI_ENDPOINT": La URL de tu recurso de Azure Open AI
"AZURE_OPENAI_API_KEY": La clave de acceso a tu recurso de Azure Open AI
"DEPLOYMENT_SLOT_NAME": El nombre del slot de deployment de tu modelo generativo de texto
"DALLE_SLOT_NAME": El nombre del slot de deployment de tu modelo generativo de imagen
"MAX_TOKENS": El número máximo de tokens que se pueden generar en una respuesta 
"TEMPERATURE": Controla la aleatoriedad de las respuestas generadas (0.0 - 1.0)

# Consideraciones
- Este proyecto es un experimento por lo tanto el código puede que no sea el más eficiente o el más elegante. Si tienes sugerencias de mejora, puedes contribuir :)
- El codigo no realiza todas las validaciones que debería, por lo que puede que no funcione correctamente si no se cumplen ciertas condiciones. 

# Roadmap / Próximos pasos
En esta sección dejaré ideas que tengo para próximas versiones:

- Probar la libreria OpenAI de C# (https://www.nuget.org/packages/OpenAI/)
- Portar la aplicación a ASP.NET y Blazor para que pueda ser ejecutada en un navegador
- Crear una versión de la aplicación para Android, Windows y iOS con .NET Maui
- Que el escenario pueda escanearse directo de una imagen
- Lectura de los relatos en voz alta

# English version
This repository is an "experiment" with which I want to achieve:
- Learn how Azure Open AI service works, how to manage it, and how to integrate it into a .NET code
- Have a "sandbox" to test new versions and features of Azure Open AI and .NET
- Create an open-source project that can be useful for other developers who want to integrate Azure Open AI into their .NET applications
- Make it something out of the ordinary and fun, that's why I chose a game :)

# Why a console application?
Mainly because it's a test project and because it's easier to implement and test. Also, it has that "vintage" feel that reminds me of old text-based adventure games.

That being said, my goal is to expand the "Adventure AI" multiverse to other types of applications, such as web applications, mobile applications, etc. All, of course, developed in .NET.

# Requirements
- .NET 8.0 installed
- An Azure subscription with access to Azure Open AI (see below)
- Deployments in this subscription of a text generative model like GPT and an image generative model like DALL-E

# How to get an Azure subscription with access to Azure Open AI
- If you don't have an Azure subscription, you can get a free one at https://azure.microsoft.com/en-us/free/
- Once you have your subscription, you can activate the Azure Open AI service by following the instructions at https://learn.microsoft.com/en-us/legal/cognitive-services/openai/limited-access

# Resource configuration
The following environment variables must be configured:
"AZURE_OPENAI_ENDPOINT": The URL of your Azure Open AI resource
"AZURE_OPENAI_API_KEY": The access key to your Azure Open AI resource
"DEPLOYMENT_SLOT_NAME": The name of the deployment slot for your text generative model
"DALLE_SLOT_NAME": The name of the deployment slot for your image generative model
"MAX_TOKENS": The maximum number of tokens that can be generated in a response
"TEMPERATURE": Controls the randomness of the generated responses (0.0 - 1.0)

# Considerations
- This project is an experiment, so the code may not be the most efficient or elegant. If you have suggestions for improvement, you can contribute :)
- The code does not perform all the validations it should, so it may not work correctly if certain conditions are not met.

# Roadmap / Next steps
In this section, I will leave ideas I have for future versions:
- Test the OpenAI C# library (https://www.nuget.org/packages/OpenAI/)
- Port the application to ASP.NET and Blazor so that it can be run in a browser
- Create a version of the application for Android, Windows, and iOS with .NET Maui
- Allow the scenario to be scanned directly from an image
- Text-to-speech reading of the stories



