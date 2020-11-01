unit uProperties;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
    var FieldX: Integer;

    var FieldY: Integer;

    /// <summary>
    /// This is a property used for testing.
    /// </summary>
    property PropertyX: Integer read FieldX;

    property PropertyY: Integer write FieldY;

    property PropertyXY: Integer read FieldY write FieldX;
  end;

implementation

end.
