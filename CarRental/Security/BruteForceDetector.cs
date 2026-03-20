
using System.Collections.Concurrent;

namespace CarRental.Security
{
public class BruteForceDetector
{
private static ConcurrentDictionary<string,int> attempts=new();

public bool IsBlocked(string ip)
{
attempts.AddOrUpdate(ip,1,(k,v)=>v+1);
return attempts[ip] > 10;
}
}
}
