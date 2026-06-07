# 🚀 Guía de Ejecución: Cómo hacer correr el proyecto PuntoVenta

Esta guía te explicará paso a paso cómo iniciar el sistema Punto de Venta en tu máquina local. El sistema está dividido en un Backend (API) y un Frontend (Blazor), y requiere de una base de datos para funcionar.

---

## 1. 📋 Requisitos Previos

Asegúrate de tener instalado lo siguiente en tu computadora:
- **[SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)** o superior.
- **Base de Datos:**
  - Si prefieres **SQL Server**: Tener una instancia local o remota ejecutándose.
  - Si prefieres **Oracle**: Tener Docker Desktop instalado para levantar el contenedor (Ver archivo `README-ORACLE.md`).
- **Git** (Opcional, para control de versiones).

---

## 2. 🗄️ Configurar la Base de Datos

### Opción A: Usando SQL Server (Recomendado por defecto)
1. Abre el archivo de configuración de la API ubicado en: `PuntoVenta.API/appsettings.json`.
2. Actualiza la cadena de conexión (`DefaultConnection`) con los datos de tu servidor SQL Server:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TU_SERVIDOR_SQL;Database=PuntoVentaDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
3. **Aplicar Migraciones:**
   Abre una terminal en la raíz del proyecto y ejecuta:
   ```bash
   dotnet ef database update --project PuntoVenta.Infrastructure --startup-project PuntoVenta.API
   ```
   *(Si no tienes la herramienta instalada, ejecuta primero: `dotnet tool install --global dotnet-ef`)*

### Opción B: Usando Oracle Database
Sigue los pasos detallados en el archivo [README-ORACLE.md](./README-ORACLE.md) para levantar el contenedor Docker de Oracle, configurar el usuario y aplicar las migraciones correspondientes.

---

## 3. ⚙️ Iniciar el Servidor (Backend / API)

El servidor es el corazón del proyecto. Provee los datos a las pantallas.
1. Abre una terminal (o consola de comandos).
2. Navega a la carpeta de la API:
   ```bash
   cd PuntoVenta.API
   ```
3. Ejecuta el proyecto:
   ```bash
   dotnet run
   ```
   *El servidor se iniciará y estará escuchando peticiones (usualmente en el puerto `5291` o similar). **No cierres esta terminal**.*

---

## 4. 🖥️ Iniciar la Interfaz de Usuario (Frontend / Blazor)

Ahora iniciaremos las pantallas con las que interactúa el usuario final.
1. Abre una **nueva** terminal (manteniendo abierta la anterior).
2. Navega a la carpeta del proyecto Blazor:
   ```bash
   cd PuntoVenta.Blazor
   ```
3. Ejecuta el proyecto:
   ```bash
   dotnet run
   ```
4. El navegador web se abrirá automáticamente cargando el sistema Punto de Venta.

---

## 5. 🔑 Usuarios de Prueba

Una vez que el sistema esté corriendo en el navegador, podrás iniciar sesión con los siguientes usuarios predeterminados (creados automáticamente al aplicar las migraciones):

| Rol | Correo Electrónico | Contraseña |
| --- | --- | --- |
| **Administrador** | `admin@puntoventa.local` | `Admin123*` |
| **Vendedor (Cajero)** | `seller@puntoventa.local` | `Seller123*` |

---

## 🛑 Cómo apagar el sistema
Para detener el sistema, simplemente ve a las dos ventanas de terminal que abriste (la de la API y la de Blazor) y presiona la combinación de teclas `Ctrl + C`.
