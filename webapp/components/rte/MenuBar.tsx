import { BoldIcon, CodeBracketIcon, ItalicIcon, StrikethroughIcon } from "@heroicons/react/24/solid";
import { Button } from "@heroui/button";
import { Editor, useEditorState } from "@tiptap/react";

type Props ={
    editor: Editor | null;
}

export default function MenuBar({editor} : Props) {
  const editorState = useEditorState({
    editor,
    selector: ({editor}) =>{
      if (!editor) return null;

      return{
        isBold: editor.isActive("bold"),
        isItalic: editor.isActive("italic"),
        isstrike: editor.isActive("strike"),
        isCodeBlock: editor.isActive("codeBlock"),        
      };
    }        
  });

  if(!editor || !editorState) return null;

  const options =[
    {
      icon:<BoldIcon className="w-5 h-5"/>,
      onclick: () => editor.chain().focus().toggleBold().run(),
      pressed: editorState.isBold
    },
    {
      icon:<ItalicIcon className="w-5 h-5"/>,
      onclick: () => editor.chain().focus().toggleItalic().run(),
      pressed: editorState.isItalic
    },
    {
      icon:<StrikethroughIcon className="w-5 h-5"/>,
      onclick: () => editor.chain().focus().toggleStrike().run(),
      pressed: editorState.isstrike
    },
    {
      icon:<CodeBracketIcon className="w-5 h-5"/>,
      onclick: () => editor.chain().focus().toggleCodeBlock().run(),
      pressed: editorState.isCodeBlock
    }
  ];


  return (
    <div className="rounded-md s´pace-x-1 pb-1 z-50">
      {options.map((option,index)=>(
        <Button
          key={index}
          type="button"
          radius="sm"
          size="sm"
          isIconOnly
          color={option.pressed ? "primary" : "default"}
          onPress={option.onclick}
        >
          {option.icon}
        </Button>
      ))}            
    </div>
  );
}
