using System.Text.Json.Serialization;

namespace Server.Models.TradingJournal.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PositionType
{
  Long,
  Short
}