"use client";

import { useTagStore } from "@/lib/useTagStore";
import { Form } from "@heroui/form";
import { Input } from "@heroui/input";
import { Button } from "@heroui/button";
import { Select, SelectItem } from "@heroui/select";
import { questionSchema, QuestionSchema } from "@/lib/schemas/questionSchema";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import RichTextEditor from "@/components/rte/RichTextEditor";
import clsx from "clsx";
import { useRouter } from "next/navigation";
import { postQuestion } from "@/lib/actions/question-actions";
import { handleError } from "@/lib/util";

export default function QuestionForm() {
  const tags = useTagStore(state => state.tags);
  const {register, control, handleSubmit, formState: {isSubmitting, isValid, errors}} = useForm<QuestionSchema>({
    resolver: zodResolver(questionSchema),
    mode: "onTouched"
  });

  const router = useRouter();

  const onSubmit = async (data: QuestionSchema) =>{
    const {data: question, error} = await postQuestion(data);    
    if(error) handleError(error);
    if(question) router.push(`/questions/${question.id}`);
  }; 

  return (
    <Form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-3 p-6 shadow-xl bg-white dark:bg-black">
      <div className="flex flex-col gap-3 w-full">
        <h3 className="text-2xl font-semibold">Title</h3>
        <Input
          {...register("title")}
          type="text"
          className="w-full"
          label="Be specific and imagine you are asking a question to another person"
          labelPlacement="outside-top"
          placeholder="e.g how would you truncate text in tailwind"
          isInvalid={!!errors.title}
          errorMessage={errors.title?.message}
        />
      </div>
      <div className="flex flex-col gap-3 w-full">
        <h3 className="text-2xl font-semibold">Body</h3>
        <Controller
          control={control}
          name="content"
          render={({field: {onChange, onBlur, value}, fieldState}) =>(
            <>
              <p className={clsx ("text-sm",{
                "text-danger": fieldState.error?.message
              })}>
                Include all the information someone would need to answer the question
              </p>
              <RichTextEditor
                onChange ={onChange}
                onBlur = {onBlur}
                value = {value || ""}
                errorMessage={fieldState.error?.message}
              />
              {fieldState.error?.message && (
                <span className="text-xs text-danger -mt-1">
                  {fieldState.error.message}
                </span>
              )}
            </>
          )}
        />        
      </div>
      <div className="flex flex-col gap-3 w-full">
        <h3 className="text-2xl font-semibold">Tags</h3>
        <p className="text-sm">Add up to 5 tags to describe what your question is about</p>  
        {tags.length > 0 &&
                <Controller
                  name='tags'
                  control={control}
                  render={({field, fieldState}) =>(
                    <Select
                      className='w-full'
                      label='Select 1-5 tags'
                      selectionMode='multiple'
                      isClearable
                      disallowEmptySelection
                      items={tags}
                      onBlur={field.onBlur}
                      selectedKeys={field.value ?? []}
                      onSelectionChange={(keys) => field.onChange(Array.from(keys))}
                      isInvalid={fieldState.invalid}
                      errorMessage={fieldState.error?.message}
                    >
                      {(tag) => <SelectItem key={tag.id}>{tag.name}</SelectItem>}
                    </Select>
                  )}
                />}       
      </div>
      <Button 
        isLoading={isSubmitting}
        isDisabled={!isValid}
        type="submit"
        color="primary"
        className="w-fit"
      >
        Post your question
      </Button>
    </Form>
  );
}
