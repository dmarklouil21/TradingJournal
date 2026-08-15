namespace Server.Models.Enums;

public enum SystemPhase
{
  PhaseOne,
  PhaseTwo,
  PhaseThree,
  PhaseFour
}

public static class SystemPhaseExtension
{
  public static string GetDescription(this SystemPhase systemPhase)
  {
    return systemPhase switch
    {
      SystemPhase.PhaseOne => "The Accumulation Engine",
      SystemPhase.PhaseTwo => "The House Money Milestone",
      SystemPhase.PhaseThree => "The Technical Overextension Warning",
      SystemPhase.PhaseFour => "The Cool-Down & Restart Rule", 
      _ => systemPhase.ToString()
    };
  }
}