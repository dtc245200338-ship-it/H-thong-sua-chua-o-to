using System.Runtime.InteropServices;
using System.Text.Json;
namespace GarageAI;
// SQLite bundled with Windows. Prepared statements keep user data out of SQL text.
public sealed class Store : IDisposable {
    IntPtr db;
    public readonly object Gate = new();
    public Store(string path) {
        if (sqlite3_open(path, out db) != 0) throw new Exception("Cannot open SQLite database");
        Exec("PRAGMA journal_mode=WAL;");
        Exec("CREATE TABLE IF NOT EXISTS GarageState (Id INTEGER PRIMARY KEY CHECK(Id=1), Data TEXT NOT NULL);");
    }
    public State Load() {
        Check(sqlite3_prepare_v2(db,"SELECT Data FROM GarageState WHERE Id=1",-1,out var s,IntPtr.Zero));
        try { return sqlite3_step(s)==100 ? JsonSerializer.Deserialize<State>(Marshal.PtrToStringUTF8(sqlite3_column_text(s,0))!)! : new State(); }
        finally { sqlite3_finalize(s); }
    }
    public void Save(State state) {
        Check(sqlite3_prepare_v2(db,"INSERT INTO GarageState(Id,Data) VALUES(1,?1) ON CONFLICT(Id) DO UPDATE SET Data=excluded.Data",-1,out var s,IntPtr.Zero));
        try { Check(sqlite3_bind_text(s,1,JsonSerializer.Serialize(state),-1,new IntPtr(-1))); if(sqlite3_step(s)!=101) throw new Exception("SQLite write failed"); }
        finally { sqlite3_finalize(s); }
    }
    void Exec(string sql) { Check(sqlite3_exec(db,sql,IntPtr.Zero,IntPtr.Zero,out var error)); if(error!=IntPtr.Zero) sqlite3_free(error); }
    void Check(int result) { if(result!=0) throw new Exception("SQLite error " + result); }
    public void Dispose()=>sqlite3_close(db);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_open([MarshalAs(UnmanagedType.LPUTF8Str)] string path,out IntPtr db);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_close(IntPtr db);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_exec(IntPtr db,[MarshalAs(UnmanagedType.LPUTF8Str)] string sql,IntPtr cb,IntPtr arg,out IntPtr error);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern void sqlite3_free(IntPtr p);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_prepare_v2(IntPtr db,[MarshalAs(UnmanagedType.LPUTF8Str)] string sql,int len,out IntPtr statement,IntPtr tail);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_bind_text(IntPtr s,int i,[MarshalAs(UnmanagedType.LPUTF8Str)] string text,int len,IntPtr destructor);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_step(IntPtr s);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern IntPtr sqlite3_column_text(IntPtr s,int col);
    [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_finalize(IntPtr s);
}
public class State {
 public List<Account> Users {get;set;}=[];
 public List<Customer> Customers {get;set;}=[];
 public List<Vehicle> Vehicles {get;set;}=[];
 public List<Catalog> Catalog {get;set;}=[];
 public List<Appointment> Appointments {get;set;}=[];
 public List<Repair> Repairs {get;set;}=[];
 public List<Invoice> Invoices {get;set;}=[];
 public int NextId {get;set;}=100;
}
public record Account(int Id,string Username,string Name,string Role,string Hash);
public record Customer(int Id,string Name,string Phone);
public record Vehicle(int Id,int CustomerId,string Plate,string Model);
public record Catalog(int Id,string Name,string Kind,decimal Price,int Stock,string Description);
public record Appointment(int Id,int VehicleId,DateTime At,string Note,string Status);
public record Line(int CatalogId,string Name,string Kind,int Quantity,decimal Price) { public decimal Total=>Quantity*Price; }
public record Repair(int Id,int VehicleId,int TechnicianId,string Status,string Note,DateTime Created,List<Line> Lines);
public record Invoice(int Id,int RepairId,decimal Total,DateTime Created,DateTime? PaidAt,string Method);
