namespace Ashy.Wpa2Decoder.Library;

/// Total process has several rounds. Each round has several steps. Progress on each step is reported in ticks
public interface IProgressBar : IDisposable
{
    void Report(long currentTicks, string additionalInfo);
    long TotalTicks { get; set; }
    void SetRoundAndStep(int totalRounds, int round, int step);
}