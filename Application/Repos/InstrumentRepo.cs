using Domain.Entities;
using Domain.Interfaces;

namespace Application.Repos;

public class InstrumentRepo :  IInstrumentRepo
{
    private readonly List<Instrument> _instruments = new ();
    private readonly Lock _locker = new();
    
    public Instrument? GetInstrumentByIsin(string isin) => _instruments.FirstOrDefault(i => i.Isin == isin);

    public Instrument? GetInstrumentByTicker(string ticker) => _instruments.FirstOrDefault(i => i.Ticker == ticker);

    public Instrument? GetInstrumentByUid(string uid) => _instruments.FirstOrDefault(x=>x.UId == uid);
    
    public IEnumerable<Instrument> GetInstruments() => _instruments.ToList();

    public void AddInstrument(Instrument instrument)
    {
        lock (_locker)
        {
            _instruments.Add(instrument);
        }
    }

    public void AddInstrument(IEnumerable<Instrument> instruments)
    {
        lock (_locker)
        {
            _instruments.AddRange(instruments);
        }
    }

    public void DeleteInstrument(Instrument instrument)
    {
        lock (_locker)
        {
            _instruments.Remove(instrument);
        }
    }

    public void DeleteInstrument(IEnumerable<Instrument> instruments)
    {
        lock (_locker)
        {
            _instruments.RemoveAll(instruments.Contains);
        }
    }
}