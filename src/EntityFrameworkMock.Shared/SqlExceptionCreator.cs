﻿/*
 * Copyright 2017-2019 Wouter Huysentruit
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.Serialization;

namespace EntityFrameworkMock
{
    public static class SqlExceptionCreator
    {
        public static SqlException Create(string message, int errorCode)
        {
            SqlException exception = Instantiate<SqlException>();
            SetProperty(exception, "_message", message);

            var errors = new ArrayList();
            var errorCollection = Instantiate<SqlErrorCollection>();
            SetProperty(errorCollection, "errors", errors);

            var error = Instantiate<SqlError>();
            SetProperty(error, "number", errorCode);
            errors.Add(error);

            SetProperty(exception, "_errors", errorCollection);
            return exception;
        }

        private static T Instantiate<T>() where T : class
        {
            return FormatterServices.GetUninitializedObject(typeof(T)) as T;
        }

        private static void SetProperty<T>(T targetObject, string fieldName, object value)
        {
            var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(targetObject, value);
            }
            else
            {
                throw new InvalidOperationException("No field with name " + fieldName);
            }
        }
    }
}
