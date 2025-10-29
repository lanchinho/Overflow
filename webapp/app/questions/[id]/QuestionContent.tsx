import { Question } from "@/lib/types";
import VotingButtons from "./VotingButtons";
import QuestionFooter from "./QuestionFooter";

type Props ={
    question: Question;
}

export default function QuestionContent({question}: Props) {
  return (
    <div className="flex border-b pb-3 px-6">
      <VotingButtons 
        target={question}
      />      
      <div className="flex flex-col w-full">
        <div className="flex-1 mt-4 ml-6 prose dark:prose-invert max-w-none"
          dangerouslySetInnerHTML={{__html: question.content}}
        />
        <QuestionFooter question={question}/>
      </div>            
    </div>
  );
}
