namespace json;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Title { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Extension { get; set; }
    public string PhotoPath { get; set; }
    public string Notes { get; set; }
    public string PhoneNumber { get; set; }
    public string FaxNumber { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}

public class Organization
{
    public string Name { get; set; }
    public List<Person> People { get; set; }
    public List<Department> Departments { get; set; }
}

public class Department
{
    public string Name { get; set; }
    public List<Person> People { get; set; }
    public List<Person> Managers { get; set; }

    public Office Office { get; set; }
}

public class Office
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string HomePage { get; set; }
    public string Extension { get; set; }
    public string Notes { get; set; }
    public string PhotoPath { get; set; }
    public string PhoneNumber { get; set; }

    public List<Person> People { get; set; }
}