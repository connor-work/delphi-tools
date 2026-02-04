unit uInterfaceMembers;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  InterfaceX = interface(IInterface)
    ['{6D5D8C25-12EC-42F8-BAFC-3BFAC05837E5}']
    function GetX: Integer;

    procedure SetX(ParamX: Integer);

    /// <summary>
    /// This is a property used for testing.
    /// </summary>
    property PropertyX: Integer read GetX write SetX;
  end;

implementation

end.
