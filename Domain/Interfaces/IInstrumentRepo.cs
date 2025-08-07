using Domain.Entities;

namespace Domain.Interfaces;

public interface IInstrumentRepo
{
    Instrument? GetInstrumentByIsin(string isin);
    Instrument? GetInstrumentByTicker(string ticker);
    Instrument? GetInstrumentByUid(string uid);

    IEnumerable<Instrument> GetInstruments();

    void AddInstrument(Instrument instrument);
    void AddInstrument(IEnumerable<Instrument> instruments);

    void DeleteInstrument(Instrument instrument);
    void DeleteInstrument(IEnumerable<Instrument> instruments);
}