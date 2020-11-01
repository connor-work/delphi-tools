unit uNestedTypes;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
    type
      /// <summary>
      /// This is an empty nested class used for testing.
      /// </summary>
      ClassY = class
      end;

    private type
      /// <summary>
      /// This is a nested enumerated type used for testing.
      /// </summary>
      EnumX = (
        exValueX
      );
  end;

implementation

end.
