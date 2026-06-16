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
- .NET 9.0 instalado
- Una suscripción a Azure con acceso a Azure Open AI (ver debajo)
- Deployments en esta suscripcion de un modelo generativo de texto como GPT y un modelo generativo de imagen como DALL-E
- **NUEVO**: Opcionalmente, acceso a Sora para generación de videos (ver configuración)

# Cómo obtener una suscripción a Azure con acceso a Azure Open AI
- Si no tienes una suscripción a Azure, puedes obtener una gratis en https://azure.microsoft.com/es-es/free/
- Una vez tengas tu suscripción, puedes activar el servicio de Azure Open AI siguiendo las instrucciones en https://learn.microsoft.com/en-us/legal/cognitive-services/openai/limited-access

# Configuración de los recursos

## Configuración
El proyecto carga configuración en este orden: `appsettings.json` opcional, `appsettings.{DOTNET_ENVIRONMENT}.json` opcional, user-secrets y variables de entorno. Las variables de entorno pisan los valores anteriores.

Para desarrollo local puedes crear `aventura-ia/appsettings.json` usando `appsettings.example.json` como plantilla:

```json
{
  "Settings": {
    "AzureOpenAiEndpoint": "https://tu-recurso.openai.azure.com/",
    "AzureOpenAiApiKey": "tu-clave-de-api",
    "AzureOpenAiDeploymentId": "tu-deployment-gpt",
    "DalleEndpoint": "https://tu-recurso-dalle.openai.azure.com/",
    "DalleApiKey": "tu-clave-dalle",
    "DalleDeploymentId": "tu-deployment-dalle",
    "SoraEndpoint": "https://tu-recurso-sora.cognitiveservices.azure.com/",
    "SoraApiKey": "tu-clave-sora",
    "SoraDeploymentId": "sora",
    "ImageGenerationTimeoutSeconds": 120,
    "VideoRequestTimeoutSeconds": 60,
    "EnableVideoPolling": false,
    "VideoPollingIntervalSeconds": 10,
    "VideoPollingTimeoutSeconds": 600
  }
}
```

Para guardar secretos fuera del archivo local:

```bash
cd aventura-ia
dotnet user-secrets set "Settings:AzureOpenAiEndpoint" "https://tu-recurso.openai.azure.com/"
dotnet user-secrets set "Settings:AzureOpenAiApiKey" "tu-clave-de-api"
dotnet user-secrets set "Settings:AzureOpenAiDeploymentId" "tu-deployment-gpt"
dotnet user-secrets set "Settings:DalleEndpoint" "https://tu-recurso-dalle.openai.azure.com/"
dotnet user-secrets set "Settings:DalleApiKey" "tu-clave-dalle"
dotnet user-secrets set "Settings:DalleDeploymentId" "tu-deployment-dalle"
```

La configuración de DALL-E solo es requerida si generas imágenes. Sora solo es requerido si ejecutas con `--video`. También puedes usar variables de entorno con doble guion bajo, por ejemplo `Settings__AzureOpenAiApiKey`, `Settings__DalleApiKey` o `Settings__SoraApiKey`.

## 🎬 Nueva funcionalidad: Generación de videos con Sora

Ahora puedes generar videos cinematográficos del escenario inicial usando Sora:

```bash
# Ejecutar con generación de video
dotnet run spanish --video

# Solo texto e imágenes (modo tradicional)
dotnet run spanish

# Solo texto, sin imágenes ni video
dotnet run spanish --text-only

# Texto sin imágenes, pero permitiendo video si agregas --video
dotnet run spanish --no-images
```

### Configuración de Sora
Para usar videos, configura las credenciales de Sora en tu `appsettings.json`. Sora está disponible en Azure OpenAI en modo preview. Si quieres que la app espere el resultado del job, usa `EnableVideoPolling: true`; si queda en `false`, se devuelve el Job ID como antes.

## 🎮 Nueva Interfaz Interactiva

El juego ahora cuenta con una interfaz de menús completamente interactiva:

### Navegación con Teclado
- **↑↓** - Navegar entre opciones
- **Enter** - Seleccionar opción
- **Escape** - Salir del juego

### Menús Disponibles
1. **Número de rondas**: Desde juegos rápidos (1 ronda) hasta aventuras épicas (10+ rondas)
2. **Opciones por ronda**: De 2 opciones (fácil) hasta 5+ opciones (experto)
3. **Sistema de pistas**: Desde modo hardcore (sin pistas) hasta ayuda completa
4. **Nivel de dificultad**: Desde opciones obvias hasta desafíos extremos
5. **Estilo de gráficos**: Múltiples estilos con emojis descriptivos

### Personalización Avanzada
Cada menú incluye una opción **"✏️ Otro - Personalizar"** que permite:
- Ingresar valores personalizados en tiempo real
- Validación automática de entradas
- Valores por defecto seguros en caso de error

## 🔧 Configuración de Menús

Las opciones de los menús se pueden personalizar editando `aventura-ia/config/menu-options.json`:

```json
{
  "rounds": {
    "1 ronda - Juego rápido": 1,
    "3 rondas - Juego estándar": 3,
    "5 rondas - Juego largo": 5,
    "7 rondas - Aventura épica": 7,
    "✏️ Otro - Personalizar": "custom"
  },
  "graphics": {
    "🎨 Ilustración - Estilo artístico": "illustration",
    "📷 Realista - Fotografía": "realistic",
    "🕹️ Retro - Pixel Art 8-bit": "8 bit pixel art",
    "🖼️ Anime - Estilo japonés": "anime style",
    "🌟 Fantasy - Arte fantástico": "fantasy art",
    "✏️ Otro - Personalizar": "custom"
  }
}
```

**💡 Tip**: Puedes agregar tus propias opciones editando este archivo. El juego las detectará automáticamente.

**📚 Documentación completa**: Ver `aventura-ia/config/README-CONFIG.md` para instrucciones detalladas.

## Ejemplo de salida con video
```
🎬 Modo de generación de video activado!
Selected Language: spanish
Bienvenido a la Aventura IA

📝 Prompt del video: Create a cinematic video scene of: En el espacio cerca de Marte...
🔄 Enviando solicitud a Sora...
🔗 URL: https://tu-recurso.cognitiveservices.azure.com/openai/v1/video/generations/jobs?api-version=preview
✅ Job de video enviado exitosamente!
🎬 Job ID: job_abc123...
⏳ El video se está procesando. Consulta el estado del job para obtener el resultado.
```

# Consideraciones
- Este proyecto es un experimento por lo tanto el código puede que no sea el más eficiente o el más elegante. Si tienes sugerencias de mejora, puedes contribuir :)
- El codigo no realiza todas las validaciones que debería, por lo que puede que no funcione correctamente si no se cumplen ciertas condiciones.
- **Importante**: El archivo `appsettings.json` está en `.gitignore` para proteger tus credenciales. Usa `appsettings.example.json` como plantilla.
- La generación de videos con Sora puede tomar varios minutos y tiene costos asociados. 

# Funcionalidades disponibles
- ✅ **Juego de aventuras por texto** con IA generativa
- ✅ **Generación de imágenes** con DALL-E para cada escena
- ✅ **Multiidioma** con traducciones automáticas
- ✅ **Sistema de pistas** inteligente
- ✅ **Generación de videos** con Sora (usando `--video`)
- ✅ **Polling opcional de jobs de video** con `EnableVideoPolling`
- ✅ **Modo solo texto** con `--text-only` y modo sin imágenes con `--no-images`
- ✅ **Configuración por JSON** y variables de entorno
- ✅ **Interfaz de menús interactiva** con navegación por flechas
- ✅ **Opciones personalizables** editables desde archivos de configuración

# Uso

## Modo básico (texto + imágenes)
```bash
cd aventura-ia
dotnet run spanish
```

## Modo con videos cinematográficos 🎬
```bash
cd aventura-ia
dotnet run spanish --video
```

## Modo solo texto
```bash
cd aventura-ia
dotnet run spanish --text-only
```

## Modo sin imágenes
```bash
cd aventura-ia
dotnet run spanish --no-images
```

## Idiomas soportados
- `spanish` - Español
- `english` - Inglés  
- `french` - Francés
- O cualquier idioma que especifiques

# Roadmap / Próximos pasos
En esta sección dejaré ideas que tengo para próximas versiones:

- ✅ ~~Integración con Sora para videos~~ **¡Completado!**
- ✅ ~~Interfaz de menús interactiva~~ **¡Completado!**
- ✅ ~~Configuración personalizable de opciones~~ **¡Completado!**
- Probar la libreria OpenAI de C# (https://www.nuget.org/packages/OpenAI/)
- Portar la aplicación a ASP.NET y Blazor para que pueda ser ejecutada en un navegador
- Crear una versión de la aplicación para Android, Windows y iOS con .NET Maui
- Que el escenario pueda escanearse directo de una imagen
- Lectura de los relatos en voz alta
- ✅ ~~Sistema de polling para verificar el estado de los videos de Sora~~ **¡Completado!**
- Cacheo local de videos generados
- Temas personalizables para la interfaz de consola
- Modo multijugador local
- Sistema de logros y estadísticas

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
- .NET 10.0 installed
- An Azure subscription with access to Azure Open AI (see below)
- Deployments in this subscription of a text generative model like GPT and an image generative model like DALL-E
- **NEW**: Optionally, access to Sora for video generation (see configuration)

# How to get an Azure subscription with access to Azure Open AI
- If you don't have an Azure subscription, you can get a free one at https://azure.microsoft.com/en-us/free/
- Once you have your subscription, you can activate the Azure Open AI service by following the instructions at https://learn.microsoft.com/en-us/legal/cognitive-services/openai/limited-access

# Resource configuration

## Configuration
The project loads configuration in this order: optional `appsettings.json`, optional `appsettings.{DOTNET_ENVIRONMENT}.json`, user-secrets, and environment variables. Environment variables override previous values.

For local development, you can create `aventura-ia/appsettings.json` using `appsettings.example.json` as a template:

```json
{
  "Settings": {
    "AzureOpenAiEndpoint": "https://your-resource.openai.azure.com/",
    "AzureOpenAiApiKey": "your-api-key",
    "AzureOpenAiDeploymentId": "your-gpt-deployment",
    "DalleEndpoint": "https://your-dalle-resource.openai.azure.com/",
    "DalleApiKey": "your-dalle-key",
    "DalleDeploymentId": "your-dalle-deployment",
    "SoraEndpoint": "https://your-sora-resource.cognitiveservices.azure.com/",
    "SoraApiKey": "your-sora-key",
    "SoraDeploymentId": "sora",
    "ImageGenerationTimeoutSeconds": 120,
    "VideoRequestTimeoutSeconds": 60,
    "EnableVideoPolling": false,
    "VideoPollingIntervalSeconds": 10,
    "VideoPollingTimeoutSeconds": 600
  }
}
```

To keep secrets outside the local file:

```bash
cd aventura-ia
dotnet user-secrets set "Settings:AzureOpenAiEndpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "Settings:AzureOpenAiApiKey" "your-api-key"
dotnet user-secrets set "Settings:AzureOpenAiDeploymentId" "your-gpt-deployment"
dotnet user-secrets set "Settings:DalleEndpoint" "https://your-dalle-resource.openai.azure.com/"
dotnet user-secrets set "Settings:DalleApiKey" "your-dalle-key"
dotnet user-secrets set "Settings:DalleDeploymentId" "your-dalle-deployment"
```

DALL-E configuration is required only when generating images. Sora is required only when running with `--video`. You can also use environment variables with double underscores, for example `Settings__AzureOpenAiApiKey`, `Settings__DalleApiKey`, or `Settings__SoraApiKey`.

## 🎬 New Feature: Video Generation with Sora

You can now generate cinematic videos of the initial scenario using Sora:

```bash
# Run with video generation
dotnet run english --video

# Text and images only (traditional mode)
dotnet run english
```

### Sora Configuration
To use videos, configure Sora credentials in your `appsettings.json`. Sora is available in Azure OpenAI in preview mode. Set `EnableVideoPolling: true` if you want the app to wait for the job result; when it is `false`, the app returns the Job ID as before.

## 🎮 New Interactive Interface

The game now features a completely interactive menu interface:

### Keyboard Navigation
- **↑↓** - Navigate between options
- **Enter** - Select option
- **Escape** - Exit game

### Available Menus
1. **Number of rounds**: From quick games (1 round) to epic adventures (10+ rounds)
2. **Options per round**: From 2 options (easy) to 5+ options (expert)
3. **Hint system**: From hardcore mode (no hints) to complete help
4. **Difficulty level**: From obvious options to extreme challenges
5. **Graphics style**: Multiple styles with descriptive emojis

### Advanced Customization
Each menu includes an **"✏️ Other - Customize"** option that allows:
- Enter custom values in real time
- Automatic input validation
- Safe default values in case of error

## 🔧 Menu Configuration

Menu options can be customized by editing `aventura-ia/config/menu-options.json`:

```json
{
  "rounds": {
    "1 round - Quick game": 1,
    "3 rounds - Standard game": 3,
    "5 rounds - Long game": 5,
    "7 rounds - Epic adventure": 7,
    "✏️ Other - Customize": "custom"
  },
  "graphics": {
    "🎨 Illustration - Artistic style": "illustration",
    "📷 Realistic - Photography": "realistic",
    "🕹️ Retro - Pixel Art 8-bit": "8 bit pixel art",
    "🖼️ Anime - Japanese style": "anime style",
    "🌟 Fantasy - Fantasy art": "fantasy art",
    "✏️ Other - Customize": "custom"
  }
}
```

**💡 Tip**: You can add your own options by editing this file. The game will detect them automatically.

**📚 Full documentation**: See `aventura-ia/config/README-CONFIG.md` for detailed instructions.

# Considerations
- This project is an experiment, so the code may not be the most efficient or elegant. If you have suggestions for improvement, you can contribute :)
- The code does not perform all the validations it should, so it may not work correctly if certain conditions are not met.
- **Important**: The `appsettings.json` file is in `.gitignore` to protect your credentials. Use `appsettings.example.json` as a template.
- Video generation with Sora can take several minutes and has associated costs.

# Available Features
- ✅ **Text-based adventure game** with generative AI
- ✅ **Image generation** with DALL-E for each scene
- ✅ **Multi-language** with automatic translations
- ✅ **Intelligent hint system**
- ✅ **Video generation** with Sora (using `--video`)
- ✅ **Optional video job polling** with `EnableVideoPolling`
- ✅ **Text-only mode** with `--text-only` and no-image mode with `--no-images`
- ✅ **JSON configuration** and environment variables
- ✅ **Interactive menu interface** with arrow key navigation
- ✅ **Customizable options** editable from configuration files

# Usage

## Basic mode (text + images)
```bash
cd aventura-ia
dotnet run english
```

## Cinematic video mode 🎬
```bash
cd aventura-ia
dotnet run english --video
```

## Text-only mode
```bash
cd aventura-ia
dotnet run english --text-only
```

## No-image mode
```bash
cd aventura-ia
dotnet run english --no-images
```

## Supported Languages
- `spanish` - Spanish
- `english` - English  
- `french` - French
- Or any language you specify

# Roadmap / Next steps
In this section, I will leave ideas I have for future versions:

- ✅ ~~Sora integration for videos~~ **Completed!**
- ✅ ~~Interactive menu interface~~ **Completed!**
- ✅ ~~Customizable menu options~~ **Completed!**
- Test the OpenAI C# library (https://www.nuget.org/packages/OpenAI/)
- Port the application to ASP.NET and Blazor so that it can be run in a browser
- Create a version of the application for Android, Windows, and iOS with .NET Maui
- Allow the scenario to be scanned directly from an image
- Text-to-speech reading of the stories
- ✅ ~~Polling system to check Sora video status~~ **Completed!**
- Local caching of generated videos
- Customizable themes for console interface
- Local multiplayer mode
- Achievement and statistics system
