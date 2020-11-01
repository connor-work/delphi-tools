unit uParameters;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
    procedure ProcedureX(ParamX: Integer);

    procedure ProcedureY(ParamY1: Integer; ParamY2: array of Integer);
  end;

implementation

procedure ClassX.ProcedureX(ParamX: Integer);
begin
end;

procedure ClassX.ProcedureY(ParamY1: Integer; ParamY2: array of Integer);
begin
end;

end.
