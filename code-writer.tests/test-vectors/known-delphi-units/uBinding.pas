unit uBinding;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

uses
  uAbstractBaseClass;

type
  ClassX = class(TAbstractBaseClass)
  procedure ProcedureX; virtual;

  procedure VirtualProcedure; override;
  end;

implementation

procedure ClassX.ProcedureX;
begin
end;

procedure ClassX.VirtualProcedure;
begin
end;

end.
