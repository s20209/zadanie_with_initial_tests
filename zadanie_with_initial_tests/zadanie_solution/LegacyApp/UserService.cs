using System;

namespace LegacyApp{
    public class UserService{
        public void AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId){
            if (!validateName(firstName, lastName)){
                return;
            }

            if (!validateEmail(email)){
                return;
            }

            int age = calculateAge(dateOfBirth);
            if (age < 21){
                return;
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
            
            if (client.Type == "VeryImportantClient"){
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient"){
                using (var userCreditService = new UserCreditService()){
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    creditLimit = creditLimit * 2;
                    user.CreditLimit = creditLimit;
                }
            }
            else{
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }
            
            if (user.HasCreditLimit && user.CreditLimit < 500){
                return;
            }

            UserDataAccess.AddUser(user);
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
         if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;
         return age;
    }
    }
}
