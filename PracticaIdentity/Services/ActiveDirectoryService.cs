using Microsoft.Extensions.Configuration;
using System;
using System.DirectoryServices;

using PracticaIdentity.Models;

namespace PracticaIdentity.Services
{


    public class ActiveDirectoryService
    {
        private readonly IConfiguration _configuration;

        public ActiveDirectoryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool EsValido(DatosValidacionAD UserData, out string error)
        {
            bool result = false;
            error = string.Empty;

            try
            {
                // Leer la URL de Active Directory desde appsettings.json
                string ldapPath = _configuration["ActiveDirectory"];

                using (DirectoryEntry directoryEntry = new DirectoryEntry(ldapPath))
                {
                    directoryEntry.AuthenticationType = AuthenticationTypes.Secure;
                    directoryEntry.Username = UserData.UserID;
                    directoryEntry.Password = UserData.Password;

                    using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                    {
                        directorySearcher.Filter = "(sAMAccountName=" + UserData.UserID + ")";
                        var searchResult = directorySearcher.FindOne();

                        if (searchResult != null)
                            result = true;
                        else
                        {
                            error = "Usuario no encontrado en Active Directory.";
                            result = false;
                        }
                    }
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                error = "El Active Directory rechazó la autenticación. Detalles: " + ex.Message;
                result = false;
            }
            catch (Exception ex)
            {
                error = "El Active Directory rechazó la autenticación. Detalles: " + ex.Message;
                result = false;
            }

            return result;
        }
    }

}
