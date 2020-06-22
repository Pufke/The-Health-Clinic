// File:    SecretaryDataBaseRepositoryFactory.cs
// Author:  Vaxi
// Created: Friday, May 29, 2020 5:47:48 PM
// Purpose: Definition of Class SecretaryDataBaseRepositoryFactory

using System;

namespace Repository.UserRepo
{
    public class SecretaryDataBaseRepositoryFactory : IUserRepositoryFacotory
    {
        public IUserRepository CreateIUserRepository()
        {
            throw new NotImplementedException();
        }
    }
}