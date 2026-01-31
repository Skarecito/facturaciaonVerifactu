# FacturacionVERIFACTU

Solución .NET para gestión de facturación con integración **VERIFACTU (AEAT)**, diseñada con arquitectura multi-proyecto, soporte **multi-tenant** y frontend en **Blazor Server**.

---

## 🧩 Proyectos de la solución

- **FacturacionVERIFACTU.API**  
  Backend ASP.NET Core:
  - JWT Authentication
  - Multi-tenant (por `TenantId`)
  - EF Core + PostgreSQL
  - Integración AEAT / VERIFACTU
  - Serilog + Polly

- **FacturacionVERIFACTU.Web**  
  Aplicación **Blazor Server** que consume la API.

- **FacturacionVERIFACTU.MAUI**  
  Aplicación .NET MAUI (móvil / desktop) que consume la API.

- **FacturacionVERIFACTU.Shared**  
  Modelos, DTOs y utilidades compartidas.

- **FacturacionVERIFACTU.Test**  
  Tests automatizados.

---

## ⚙️ Requisitos

- .NET SDK (recomendado: **.NET 8**)
- PostgreSQL
- Visual Studio 2022 / Rider / VS Code

---

## 🔧 Configuración (API)

Archivo:  
`FacturacionVERIFACTU.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=facturacion;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "CAMBIA_ESTO_POR_UN_SECRETO_LARGO_Y_SEGURO",
    "Issuer": "FacturacionVERIFACTU",
    "Audience": "FacturacionVERIFACTU"
  },
  "VERIFACTU": {
    "AEATUrl": "https://prewww2.aeat.es/wlpl/TGVI-SJDT/VeriFactuServiceS",
    "TimeoutSegundos": 30
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
