﻿using HRS_2._0.Repositories.Abstract;
using System.DirectoryServices;

namespace HRS_2._0.Repositories.Implementation
{
    public class ActivityDirectoryService
    {
        public class ActiveDirectoryService : IActiveDirectoryService
        {
            private readonly string _path;
            private readonly string _domain;

            public ActiveDirectoryService(string path, string domain)
            {
                _path = path;
                _domain = domain;
            }

            public bool IsAuthenticated(string domain, string username, string password, out string errorMessage)
            {
                errorMessage = string.Empty;

                if (string.IsNullOrEmpty(username))
                {
                    errorMessage = "Please enter your username";
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    errorMessage = "Please enter your password";
                    return false;
                }

                try
                {
                    string domainAndUsername = $"{_domain}\\{username}";
                    using var entry = new System.DirectoryServices.DirectoryEntry(_path, domainAndUsername, password);
                    var obj = entry.NativeObject;
                    using var search = new DirectorySearcher(entry)
                    {
                        Filter = $"(SAMAccountName={username})"
                    };

                    search.PropertiesToLoad.Add("cn");
                    var result = search.FindOne();

                    if (result == null)
                    {
                        errorMessage = "Bad Username or Password. Try again";
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return false;
                }
            }
        }
    }
}
