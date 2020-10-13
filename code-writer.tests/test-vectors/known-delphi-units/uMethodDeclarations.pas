unit uMethodDeclarations;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
  procedure ProcedureX;

  /// <summary>
  /// This is a method used for testing.
  /// </summary>
  constructor ConstructorX;

  destructor DestructorX;

  function FunctionX: Integer;
  end;

implementation

procedure ClassX.ProcedureX;
begin
end;

constructor ClassX.ConstructorX;
begin
end;

destructor ClassX.DestructorX;
begin
end;

function ClassX.FunctionX: Integer;
begin
end;

end.
