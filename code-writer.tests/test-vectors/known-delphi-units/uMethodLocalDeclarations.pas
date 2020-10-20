unit uMethodLocalDeclarations;

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
var
  lX: Integer;
  lY: string;
begin
end;

end.
