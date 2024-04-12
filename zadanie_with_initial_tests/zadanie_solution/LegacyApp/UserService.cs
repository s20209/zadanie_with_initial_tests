using System;

namespace LegacyApp{
    public class UserService{
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId){
            if (!validateName(firstName, lastName)){
                 return false;
            }

            if (!validateEmail(email)){
                return false;
            }

            int age = calculateAge(dateOfBirth);
            if (age < 21){
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = new User{
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            
            SetCreditLimit(user, client);
            
            if (user.HasCreditLimit && user.CreditLimit < 500){
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }
        private static bool validateName(string firstName, string lastName){
            return !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName);
        }
        private static bool validateEmail(string email){
            return email.Contains("@") && email.Contains(".");
        }

        private static int calculateAge(DateTime dateOfBirth){
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;
            return age;
        }

        private static void SetCreditLimit(User user, Client client)
        {
            using (var userCreditService = new UserCreditService())
            {
                int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                if (client.Type == "VeryImportantClient")
                {
                    user.HasCreditLimit = false;
                }
                else
                {
                    creditLimit = client.Type == "ImportantClient" ? creditLimit * 2 : creditLimit;
                    user.CreditLimit = creditLimit;
                    user.HasCreditLimit = true;
                }
            }
        }
    }
}
