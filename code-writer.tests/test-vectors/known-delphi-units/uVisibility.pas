unit uVisibility;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
    procedure ProcedureX;

    private procedure ProcedureY;

    protected procedure ProcedureZ;

    public procedure ProcedureA;
  end;

implementation

procedure ClassX.ProcedureX;
begin
end;

procedure ClassX.ProcedureY;
begin
end;

procedure ClassX.ProcedureZ;
begin
end;

procedure ClassX.ProcedureA;
begin
end;

end.
