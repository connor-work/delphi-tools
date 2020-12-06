unit uClassInterface;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

uses
  uBaseClass,
  uExampleInterface;

type
  ClassX = class(uBaseClass.TBaseClass, uExampleInterface.IExampleInterface)
  end;

implementation

end.
