using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ServiceTowerWeb.Models;
using Rotativa.AspNetCore;

namespace ServiceTowerWeb.Controllers;

public class ExportController : Controller
{
    private readonly string? _connectionString;
    private readonly IWebHostEnvironment _env;

    public ExportController(IConfiguration config, IWebHostEnvironment env)
    {
        _connectionString = config.GetConnectionString("Supabase") 
                            ?? config["ConnectionStrings:Supabase"];
        _env = env;
    }

    public IActionResult Descargar(string orden, string tipo)
    {
        ViewBag.Orden = orden;
        ViewBag.Tipo = tipo; 
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerarCsv(string orden)
    {
        try
        {
            var builder = new System.Text.StringBuilder();
            // 1. Encabezado con RESOLUCIÓN incluida
            builder.AppendLine("Orden,Cliente,Tecnico,ID_Equipo,Modelo,Serie,Resolucion,Area,Ubicacion,Etiqueta,Ribbon,Comentarios,FotoAntesUrl,FotoDespuesUrl");

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                // 2. SELECT con ""resolucion"" (índice 6)
                var sql = @"SELECT ""ordenServicio"", ""cliente"", ""tecnico"", ""idEquipo"", ""modelo"", 
                                   ""serie"", ""resolucion"", ""area"", ""ubicacion"", ""usoEtiqueta"", ""usoRibbon"", 
                                   ""comentarios"", ""fotoAntesUrl"", ""fotoDespuesUrl"" 
                            FROM mantenimientos 
                            WHERE ""ordenServicio"" ILIKE @o OR ""cliente"" ILIKE @o";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("o", $"%{orden}%");
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var fila = new string[] {
                                reader[0]?.ToString() ?? "",  // ordenServicio
                                reader[1]?.ToString() ?? "",  // cliente
                                reader[2]?.ToString() ?? "",  // tecnico
                                reader[3]?.ToString() ?? "",  // idEquipo
                                reader[4]?.ToString() ?? "",  // modelo
                                reader[5]?.ToString() ?? "",  // serie
                                reader[6]?.ToString() ?? "",  // <-- RESOLUCIÓN
                                reader[7]?.ToString() ?? "",  // area
                                reader[8]?.ToString() ?? "",  // ubicacion
                                reader[9]?.ToString() ?? "",  // usoEtiqueta
                                reader[10]?.ToString() ?? "", // usoRibbon
                                reader[11]?.ToString()?.Replace("\r", " ").Replace("\n", " ").Replace(",", ";") ?? "", // comentarios
                                reader[12]?.ToString() ?? "", // fotoAntesUrl
                                reader[13]?.ToString() ?? ""  // fotoDespuesUrl
                            };
                            builder.AppendLine(string.Join(",", fila.Select(f => $"\"{f}\"")));
                        }
                    }
                }
            }

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var fileBytes = bom.Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

            return File(fileBytes, "text/csv", $"Reporte_{orden}_{DateTime.Now:ddMMyy}.csv");
        }
        catch (Exception ex)
        {
            return Content("Error al generar CSV: " + ex.Message);
        }
    }
}