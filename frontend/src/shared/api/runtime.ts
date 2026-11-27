import { z } from 'zod';

function enumeration<const T extends readonly [string, ...string[]]>(names: T) {
  return z.union([
    z
      .number()
      .int()
      .min(0)
      .max(names.length - 1),
    z.enum(names).transform((name) => names.indexOf(name)),
  ]);
}
export const roleSchema = enumeration(['Student', 'Mentor', 'Tutor']);
export const profileSchema = z.object({
  id: z.uuid(),
  email: z.email(),
  role: roleSchema,
});
export const sessionSchema = profileSchema.extend({
  token: z.string().min(1),
  refreshToken: z.uuid(),
});
export const questionnaireSchema = z.object({
  firstName: z.string(),
  lastName: z.string(),
  city: z.string(),
  education: z.string(),
  goalDescription: z.string(),
  communicationFormat: enumeration(['Online', 'InPerson', 'Hybrid']),
  monthsNumberMin: z.number(),
  monthsNumberMax: z.number(),
  hoursNumberWeek: z.number(),
  skills: z.array(
    z.object({
      name: z.string(),
      skillLevel: enumeration([
        'Beginner',
        'Intermediate',
        'Advanced',
        'Expert',
      ]),
    }),
  ),
  availability: z.array(
    z.object({
      dayOfWeek: enumeration([
        'Sunday',
        'Monday',
        'Tuesday',
        'Wednesday',
        'Thursday',
        'Friday',
        'Saturday',
      ]),
      startTime: z.string(),
      endTime: z.string(),
    }),
  ),
  teacherInfo: z
    .object({
      placeOfWork: z.string(),
      position: z.string(),
      yearsExperience: z.number(),
      maxStudents: z.number(),
    })
    .nullable()
    .optional(),
});
export type Profile = z.infer<typeof profileSchema>;
export type Session = z.infer<typeof sessionSchema>;
export type Questionnaire = z.infer<typeof questionnaireSchema>;
export const mentorQuestionnaireSchema = questionnaireSchema.extend({
  firstName: z.string().nullable(),
  lastName: z.string().nullable(),
  city: z.string().nullable(),
  education: z.string().nullable(),
  goalDescription: z.string().nullable(),
  skills: questionnaireSchema.shape.skills.nullable(),
  availability: questionnaireSchema.shape.availability.nullable(),
  teacherInfo: z
    .object({
      placeOfWork: z.string().nullable(),
      position: z.string().nullable(),
      yearsExperience: z.number(),
      maxStudents: z.number(),
    })
    .nullable()
    .optional(),
});
export type MentorQuestionnaire = z.infer<typeof mentorQuestionnaireSchema>;
export const applicationSchema = z.object({
  id: z.uuid(),
  questionnaireForm: mentorQuestionnaireSchema.nullish(),
  reasonForRefusal: z.string().nullish(),
});
export const curatorApplicationSchema = z.object({
  id: z.uuid(),
  studentQuestionnaireForm: mentorQuestionnaireSchema.nullish(),
  mentorQuestionnaireForm: mentorQuestionnaireSchema.nullish(),
});
export const applicationListSchema = z.union([
  z.array(applicationSchema),
  z
    .object({ listApp: z.array(applicationSchema) })
    .transform((value) => value.listApp),
]);
export const curatorApplicationListSchema = z.union([
  z.array(curatorApplicationSchema),
  z
    .object({ listForTutorApp: z.array(curatorApplicationSchema) })
    .transform((value) => value.listForTutorApp),
]);
const applicationDetailSchema = z.object({
  id: z.uuid(),
  studentId: z.uuid(),
  mentorId: z.uuid(),
});
export const createdApplicationSchema = z.union([
  applicationDetailSchema,
  z
    .object({ appDetail: applicationDetailSchema })
    .transform((value) => value.appDetail),
]);
export type Application = z.infer<typeof applicationSchema>;
export type CuratorApplication = z.infer<typeof curatorApplicationSchema>;
const mentorCardSchema = z.object({
  userId: z.uuid(),
  questionnaireId: z.uuid(),
  firstName: z.string().nullable(),
  lastName: z.string().nullable(),
  position: z.string().nullable(),
  placeOfWork: z.string().nullable(),
  yearsExperience: z.number().int().nonnegative(),
  skills: z
    .array(z.object({ id: z.uuid(), name: z.string().nullable() }))
    .nullable(),
  rating: z.number().nullish(),
  hoursPerWeek: z.number().nonnegative(),
  availablePlaces: z.number().int().nonnegative(),
  compatibility: z.number().nullish(),
});
export const mentorSearchResultSchema = z.object({
  items: z.array(mentorCardSchema).nullable(),
  totalCount: z.number().int().nonnegative(),
  page: z.number().int().positive(),
  pageSize: z.number().int().positive(),
  totalPages: z.number().int().nonnegative(),
});
export type MentorSearchItem = z.infer<typeof mentorCardSchema>;
export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public fields: Record<string, string[]> = {},
  ) {
    super(message);
  }
}
const errorSchema = z.object({
  message: z.string().nullish(),
  detail: z.string().nullish(),
  errors: z.record(z.string(), z.array(z.string())).optional(),
});
export function decodeResponse(response: Response, source: unknown): unknown {
  let body: unknown;
  try {
    body =
      typeof source === 'string' && source.length ? JSON.parse(source) : source;
  } catch {
    throw new ApiError(response.status, 'Сервер вернул некорректный ответ.');
  }
  if (!response.ok) {
    const parsed = errorSchema.safeParse(body);
    throw new ApiError(
      response.status,
      parsed.success
        ? parsed.data.message ||
            parsed.data.detail ||
            'Не удалось выполнить запрос.'
        : 'Не удалось выполнить запрос.',
      parsed.success ? parsed.data.errors : undefined,
    );
  }
  return body;
}
