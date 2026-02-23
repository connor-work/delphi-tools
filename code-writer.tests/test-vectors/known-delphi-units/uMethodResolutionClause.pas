unit uMethodResolutionClause;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  InterfaceX = interface(IInterface)
    ['{6D5D8C25-12EC-42F8-BAFC-3BFAC05837E5}']
    procedure ProcedureX;
  end;

type
  ClassX = class(TInterfacedObject, InterfaceX)
    procedure ProcedureX2;

    procedure InterfaceX.ProcedureX = ProcedureX2;
  end;

implementation

procedure ClassX.ProcedureX2;
begin
end;

end.
