namespace WebApplicationTest.Services.Interface
{
    public interface IHashingData
    {
        string HashData(string data);
        bool VerifyData(string data, string hashData);

    }
}
