unit uClassInterface;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

uses
  uExampleInterface;

type
  ClassX = class(TInterfacedObject, uExampleInterface.IExampleInterface)
  end;

implementation

end.
