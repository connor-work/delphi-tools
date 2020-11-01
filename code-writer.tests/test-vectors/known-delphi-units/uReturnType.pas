unit uReturnType;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
    function FunctionX: Integer;

    function FunctionY(ParamY: Integer): Integer;
  end;

implementation

function ClassX.FunctionX: Integer;
begin
end;

function ClassX.FunctionY(ParamY: Integer): Integer;
begin
end;

end.
