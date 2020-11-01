unit uAttributes;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

uses
  uExampleAttribute;

type
  [Example()]
  ClassX = class
  [volatile]
  [Example]
  var FieldX: Integer;
  end;

implementation

end.
