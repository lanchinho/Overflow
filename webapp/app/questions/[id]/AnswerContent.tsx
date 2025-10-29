import { Answer } from "@/lib/types";
import VotingButtons from "./VotingButtons";
import AnswerFooter from "./AnswerFooter";
import { getCurrentUser } from "@/lib/actions/auth-actions";

type Props = {
    answer: Answer;
}
export default async function AnswerContent({answer}: Props) {
  const user = await getCurrentUser();

  return (
    <div className="flex border-b pb-3 px-6">
      <VotingButtons 
        target={answer}
        currentUserId = {user.id}
      />
      <div className="flex flex-col w-full">
        <div className="flex-1 mt-4 ml-6 prose max-w-none dark:prose-invert"
          dangerouslySetInnerHTML={{__html: answer.content}}

        />
        <AnswerFooter answer={answer} currentUser={user}/>
      </div>
    </div>
  );
}
    