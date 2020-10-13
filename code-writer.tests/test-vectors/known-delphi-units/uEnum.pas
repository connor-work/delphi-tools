unit uEnum;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  /// <summary>
  /// This is an enumerated type used for testing.
  /// </summary>
  EnumX = (
    /// <summary>
    /// This is an enumerated value used for testing, without explicitly assigned ordinality.
    /// </summary>
    exValueX,

    /// <summary>
    /// This is an enumerated value used for testing, with explicitly assigned ordinality.
    /// </summary>
    exValueY = 3
  );

implementation

end.
