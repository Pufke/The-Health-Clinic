// File:    RegisteredUser.cs
// Author:  Vaxi
// Created: Wednesday, April 1, 2020 9:43:16 PM
// Purpose: Definition of Class RegisteredUser

using System;

namespace Model.Users
{
    public class RegisteredUser : Person
    {
        private string username;
        private string password;
        private string gender;


        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                this.username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                this.password = value;
                OnPropertyChanged("Password");
            }
        }
        private string parentsName;

        public string ParentsName
        {
            get { return parentsName; }
            set { parentsName = value; }
        }


        public string Gender
        {
            get { return gender; }
            set { gender = value; }
        }
    }
}