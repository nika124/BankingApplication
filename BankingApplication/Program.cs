using BankingApplication.Infrastructure;
using BankingApplication.Models;
using BankingApplication.Repositories;

var logger = new FileLogger(Path.Combine("Logs", "errors.txt"));
var storageProvider = new JsonStorageProvider(logger);

var userRepository = new UserRepository(storageProvider);

Console.WriteLine("=== USER REPOSITORY CRUD TEST ===");

// READ
Console.WriteLine("\n1. READ USERS");
var users = userRepository.GetAll();

Console.WriteLine($"Users count: {users.Count}");
foreach (var user in users)
{
    Console.WriteLine($"{user.UserId} - {user.FirstName} {user.LastName}");
}

// ADD
Console.WriteLine("\n2. ADD USER");

var testUser = new User
{
    UserId = 999,
    FirstName = "Test",
    LastName = "User",
    PersonalNumber = "99999999999",
    Email = "test.user@example.com",
    PhoneNumber = "+995555000999",
    CreatedAt = DateTime.Now
};

if (userRepository.GetByUserId(testUser.UserId) is null)
{
    userRepository.Add(testUser);
    Console.WriteLine("User added.");
}
else
{
    Console.WriteLine("User already exists.");
}

// READ ONE
Console.WriteLine("\n3. READ ADDED USER");

var addedUser = userRepository.GetByUserId(999);

if (addedUser is not null)
{
    Console.WriteLine($"Found: {addedUser.UserId} - {addedUser.FirstName} {addedUser.LastName}");
}
else
{
    Console.WriteLine("User not found.");
}

// UPDATE
Console.WriteLine("\n4. UPDATE USER");

if (addedUser is not null)
{
    addedUser.FirstName = "Updated";
    addedUser.LastName = "User";

    var updated = userRepository.Update(addedUser);
    Console.WriteLine($"Updated: {updated}");
}

// VERIFY UPDATE
var updatedUser = userRepository.GetByUserId(999);
Console.WriteLine($"After update: {updatedUser?.FirstName} {updatedUser?.LastName}");

//// DELETE
//Console.WriteLine("\n5. DELETE USER");

//var deleted = userRepository.Delete(999);
//Console.WriteLine($"Deleted: {deleted}");

//// VERIFY DELETE
//var deletedUser = userRepository.GetByUserId(999);
//Console.WriteLine(deletedUser is null
//    ? "Delete verified: user no longer exists."
//    : "Delete failed: user still exists.");

Console.WriteLine("\n=== TEST FINISHED ===");
Console.ReadLine();