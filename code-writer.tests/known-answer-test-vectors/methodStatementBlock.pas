unit UnitX;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
  procedure ProcedureX;
  end;

implementation

procedure ClassX.ProcedureX;
begin
  writeln('Hello world 1');
  writeln('Hello world 2');
end;

end.
