unit uAbstractBaseClass;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  TAbstractBaseClass = class
  procedure VirtualProcedure; virtual; abstract;
  end;

implementation

end.
